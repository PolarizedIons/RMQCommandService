namespace RMQCommandService.RabbitMq
{
    public static class Constants
    {
        public const string SendQueueName = "RMQCommandService.bus";
        public const string ReplyToQueueName = "RMQCommandService.reply-to";
        public const int ResponseTimeoutDelay = 10000;
    }
}
