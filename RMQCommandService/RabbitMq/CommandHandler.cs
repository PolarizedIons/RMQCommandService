using System;
using Microsoft.Extensions.DependencyInjection;
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
    public class CommandHandler : IDisposable
    {
        private readonly ConnectionManager _connectionManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _serviceId;

        public CommandHandler(ConnectionManager connectionManager, IServiceProvider serviceProvider, IOptions<ConnectionDetails> connectionDetails)
        {
            _connectionManager = connectionManager;
            _serviceProvider = serviceProvider;
            _serviceId = connectionDetails.Value.ReceiveServiceId;
        }

        public void CreateHandler()
        {
            var channel = _connectionManager.CreateChannel();
            channel.QueueDeclare($"{Constants.SendQueueName}.{_serviceId}");
            channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += async (sender, e) =>
            {
                var replyProps = channel.CreateBasicProperties();
                replyProps.CorrelationId = e.BasicProperties.CorrelationId;

                dynamic? response = null;
                string? exception = null;

                try
                {
                    var request = e.Body.ToArray().DeserializeFromBinary<Request>();

                    Log.Debug("Received {@Request}", request);

                    var returnType = Type.GetType(request.ReturnType);
                    if (returnType == null)
                    {
                        throw new TypeNotFound($"Type {request.ReturnType} not found");
                    }

                    var requestType = Type.GetType(request.RequestType);
                    if (requestType == null)
                    {
                        throw new TypeNotFound($"Type {request.RequestType} not found");
                    }

                    var commandHandlerType = typeof(IConsumer<,>).MakeGenericType(requestType, returnType);
                    var handler = (dynamic) _serviceProvider.GetRequiredService(commandHandlerType);
                    var command = (dynamic) Convert.ChangeType(request.Command, requestType);

                    response = (dynamic) await handler.HandleCommand(command);

                    if (response.GetType().IsGenericType && response.GetType().GetGenericTypeDefinition()
                        .IsAssignableFrom(typeof(OneOf<,>)))
                    {
                        response = response.IsT0 ? response.AsT0 : response.AsT1;
                    }
                }
                catch (Exception ex)
                {
                    exception = ex.Message;
                }
                finally
                {
                    var resp = new Response {CommandResponse = response, Exception = exception};
                    Log.Debug("Sending to {Queue} {@Response}", e.BasicProperties.ReplyTo, resp);
                    channel.BasicPublish("", e.BasicProperties.ReplyTo, replyProps, resp.SerializeToBinary());
                    channel.BasicAck(e.DeliveryTag, false);
                }
            };

            var listenQueue = $"{Constants.SendQueueName}.{_serviceId}";
            Log.Debug("Listening on {Queue}", listenQueue);
            channel.BasicConsume(consumer, listenQueue, false);
        }

        public void Dispose()
        {
            _connectionManager.Dispose();
        }
    }
}
