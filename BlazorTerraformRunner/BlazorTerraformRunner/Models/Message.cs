namespace BlazorTerraformRunner.Models
{
    public enum MessageType
    {
        NotSpecified,
        Information,
        Error
    }
    public class Message
    {
        public required MessageType MessageType { get; set; }
        public required string Text { get; init; }
    }
}
