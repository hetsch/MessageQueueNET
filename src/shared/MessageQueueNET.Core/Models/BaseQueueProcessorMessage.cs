using System;

namespace MessageQueueNET.Core.Models;

public class BaseQueueProcessorMessage
{
    public string ProcessId { get; set; } = "";
    public string Worker { get; set; } = "";
    public string? ResultQueue { get; set; }
}
