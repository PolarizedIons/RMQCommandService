using RabbitMQ.Client;

namespace RMQCommandService.Extentions
{
    public static class RabbitMqChannelExtention
    {
        public static QueueDeclareOk QueueDeclare(this IModel channel, string channelName)
        {
            return channel.QueueDeclare(channelName, false, false, true, null);
        }
    }
}
