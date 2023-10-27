using System.Collections.Generic;

namespace MessageQueueNET.Client.Models;

public class QueuePropertiesResult : ApiResult
{
    public Dictionary<string, QueueProperties>? Queues { get; set; }
}
