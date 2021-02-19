namespace RMQCommandService.Models
{
    public struct Request
    {
        public string ReturnType { get; set; }
        public string RequestType { get; set; }
        public ICommand Command { get; set; }
    }
}
