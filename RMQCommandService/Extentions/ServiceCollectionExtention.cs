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

        public static IServiceCollection ConfigureRMQHandler(this IServiceCollection serviceCollection, HandlerServiceType serviceType = default)
        {
            serviceCollection.AddHostedService<HandlerManager>();

            foreach (var type in typeof(IConsumer<,>).GetAllInAssembly())
            {
                foreach (var iface in type.GetInterfaces().Where(i => i.GetGenericTypeDefinition() == typeof(IConsumer<,>)))
                {
                    switch (serviceType)
                    {
                        case HandlerServiceType.Singleton:
                            serviceCollection.AddSingleton(iface, type);
                            break;
                        case HandlerServiceType.Scoped:
                            serviceCollection.AddScoped(iface, type);
                            break;
                        case HandlerServiceType.Transient:
                            serviceCollection.AddTransient(iface, type);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, null);
                    }
                }
            }

            return serviceCollection;
        }
    }
}
