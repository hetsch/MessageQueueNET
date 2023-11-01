namespace MessageQueueNET.Core.Models;
public class QueueProcessorResult
{
    public bool Succeeded { get; set; } = true;
    public bool Confirm { get; set; }
    public string? ErrorMessages { get; set; }

    public object? Body { get; set; }
}
