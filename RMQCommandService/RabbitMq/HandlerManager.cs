using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace RMQCommandService.RabbitMq
{
    public class HandlerManager : IHostedService
    {
        private readonly CommandHandler _commandHandler;

        public HandlerManager(CommandHandler commandHandler)
        {
            _commandHandler = commandHandler;
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _commandHandler.CreateHandler();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _commandHandler.Dispose();
            return Task.CompletedTask;
        }
    }
}