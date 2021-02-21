using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OneOf;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RMQCommandService.Exceptions;
using RMQCommandService.Extentions;
using RMQCommandService.Models;
using Serilog;

namespace RMQCommandService.RabbitMq
{
    public class RMQBus : IDisposable
    {
        private readonly ConnectionManager _connectionManager;
        private readonly ConcurrentDictionary<string, object> _callbacks = new ConcurrentDictionary<string, object>();
        private readonly string _defaultServiceId;

        public RMQBus(ConnectionManager connectionManager, IOptions<ConnectionDetails> connectionDetails)
        {
            _connectionManager = connectionManager;
            _defaultServiceId = connectionDetails.Value.DefaultBusService;
        }

        public Task<T> Send<T>(ICommand command, string? toService = null)
        {
            toService ??= _defaultServiceId;
            var channel = _connectionManager.CreateChannel();
            channel.QueueDeclare(Constants.SendQueueName);
            var replyQueueName = $"{Constants.ReplyToQueueName}.{Guid.NewGuid()}";
            channel.QueueDeclare(replyQueueName);

            var props = channel.CreateBasicProperties();
            props.CorrelationId = Guid.NewGuid().ToString();
            props.ReplyTo = replyQueueName;

            var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            _callbacks[props.CorrelationId] = tcs;

            Task.Delay(Constants.ResponseTimeoutDelay).ContinueWith((x) =>
            {
                if (_callbacks.TryRemove(props.CorrelationId, out var _))
                {
                    channel.QueueDelete(replyQueueName);
                    tcs.SetException(new TimeoutException("Timed out trying to get response"));
                }
            });

            var request = new Request {Command = command, RequestType = command.GetType().AssemblyQualifiedName!, ReturnType = typeof(T).AssemblyQualifiedName! };

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (_, e) => 
            {
                if (!_callbacks.TryRemove(e.BasicProperties.CorrelationId, out var tcsObj))
                {
                    channel.QueueDelete(replyQueueName);
                    return;
                }

                var receivedTcs = (dynamic) tcsObj;
                try
                {
                    var result = e.Body.ToArray().DeserializeFromBinary<Response>();
                    Log.Debug("Received on {Queue} {@Response}", replyQueueName, result);

                    if (result.CommandResponse != null)
                    {
                        receivedTcs.SetResult((dynamic) result.CommandResponse);
                    }
                    else
                    {
                        receivedTcs.SetException(new ExceptionReceived($"Exception Received: --> {result.Exception}"));
                    }
                }
                catch (Exception ex)
                {
                    receivedTcs.SetException(ex);
                }
                finally
                {
                    channel.QueueDelete(replyQueueName);
                }
            };

            channel.BasicConsume(consumer, replyQueueName, true);
            var sendQueue = $"{Constants.SendQueueName}.{toService}";
            Log.Debug("Sending on {Queue} {@Request}", sendQueue, request);
            channel.BasicPublish("", sendQueue, props, request.SerializeToBinary());

            return tcs.Task;
        }

        public async Task<OneOf<TSuccess, TFailure>> Send<TSuccess, TFailure>(ICommand command, string? toService = null) where TSuccess : class where TFailure : class
        {
            return await Send<OneOf<TSuccess, TFailure>>(command, toService);
        }

        public void Dispose()
        {
            _connectionManager.Dispose();
        }
    }
}
