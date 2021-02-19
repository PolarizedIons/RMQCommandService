using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using RMQCommandService.Models;
using RMQCommandService.RabbitMq;

namespace RMQCommandService.Extentions
{

    public static class ServiceCollectionExtention
    {
        public static IServiceCollection AddRMQCommandService(this IServiceCollection serviceCollection,  Action<ConnectionDetails> connectionDetailsOptions)
        {
            serviceCollection.Configure(connectionDetailsOptions);

            serviceCollection.AddSingleton<ConnectionManager>();
            serviceCollection.AddSingleton<RMQBus>();
            serviceCollection.AddSingleton<CommandHandler>();

            return serviceCollection;
        }

        public static IServiceCollection ConfigureRMQHandler(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddHostedService<HandlerManager>();

            foreach (var type in typeof(IConsumer<,>).GetAllInAssembly())
            {
                foreach (var iface in type.GetInterfaces().Where(i => i.GetGenericTypeDefinition() == typeof(IConsumer<,>)))
                {
                    serviceCollection.AddSingleton(iface, type);
                }
            }

            return serviceCollection;
        }
    }
}
