using System;

namespace RMQCommandService.Exceptions
{
    public class TypeNotFound : Exception
    {
        public TypeNotFound(string message) : base(message) {}
    }
}
