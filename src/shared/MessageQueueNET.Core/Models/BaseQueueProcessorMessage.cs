namespace MessageQueueNET.Core.Models;

public class BaseQueueProcessorMessage
{
    public string JobId { get; set; } = "";
    public string JobType { get; set; } = "";
    public string? ResultQueue { get; set; }
}
