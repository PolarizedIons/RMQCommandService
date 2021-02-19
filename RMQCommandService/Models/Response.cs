namespace RMQCommandService.Models
{
    public struct Response
    {
        public string? Exception { get; set; }
        public object? CommandResponse { get; set; }
    }
}
