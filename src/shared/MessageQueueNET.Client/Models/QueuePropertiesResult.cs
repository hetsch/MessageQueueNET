using System.Collections.Generic;

namespace MessageQueueNET.Client.Models;

public class QueuePropertiesResult
{
    public Dictionary<string, QueueProperties>? Queues { get; set; }
}
