using System;
using System.Collections.Generic;
using System.Linq;

namespace RMQCommandService.Extentions
{
    public static class TypeExtention
    {
        public static IEnumerable<Type> GetAllInAssembly(this Type type)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => !x.IsInterface &&
                            !x.IsAbstract &&
                            (type.IsAssignableFrom(x) || x.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == type))
                );
        }
    }
}
