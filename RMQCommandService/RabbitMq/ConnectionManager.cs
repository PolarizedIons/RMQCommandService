using System;
using Microsoft.Extensions.Options;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using RMQCommandService.Models;

namespace RMQCommandService.RabbitMq
{
    public class ConnectionManager : IDisposable
    {
        private readonly ConnectionDetails _connectionDetails;
        private IConnection? _connection;

        public ConnectionManager(IOptions<ConnectionDetails> connectionDetails)
        {
            _connectionDetails = connectionDetails.Value;

            if (string.IsNullOrEmpty(connectionDetails.Value.Host))
            {
                throw new ArgumentNullException(nameof(connectionDetails.Value.Host));
            }
        }

        public IConnection OpenConnection()
        {
            if (_connection?.IsOpen == true)
            {
                return _connection;
            }

            var factory = new ConnectionFactory
            {
                HostName = _connectionDetails.Host,
                UserName = _connectionDetails.User,
                Password = _connectionDetails.Password,
            };

            Policy.Handle<BrokerUnreachableException>()
                .WaitAndRetry(5, x => TimeSpan.FromSeconds(5))
                .Execute(() => _connection = factory.CreateConnection());

            return _connection!;
        }

        public IModel CreateChannel()
        {
            if (_connection == null)
            {
                OpenConnection();
            }

            return _connection!.CreateModel();
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
