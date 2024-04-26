namespace MessageQueueNET.Core.Models;

public class QueueProcessorResult
{
    public string ProcessId { get; set; } = "";
    public string Worker { get; set; } = "";
    public bool Succeeded { get; set; } = true;
    public string? ErrorMessages { get; set; }
    public string? Publisher { get; set; }
    public string? Subject { get; set; }

    public object? Body { get; set; }
}
