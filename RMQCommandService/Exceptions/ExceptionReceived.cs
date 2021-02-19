using System;

namespace RMQCommandService.Exceptions
{
    public class ExceptionReceived : Exception
    {
        public ExceptionReceived(string message) : base(message) {}
    }
}
