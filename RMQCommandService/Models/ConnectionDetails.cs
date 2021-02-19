namespace RMQCommandService.Models
{
    public class ConnectionDetails
    {
        public string ReceiveServiceId { get; set; } = "DEFAULT";
        public string DefaultBusService { get; set; } = "DEFAULT";
        public string Host { get; set; } = null!;
        public string? User { get; set; } = "guest";
        public string? Password { get; set; } = "guest";
    }
}
