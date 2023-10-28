using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace MessageQueueNET.Client.Models;

public class QueuePropertiesResult : ApiResult
{
    public QueuePropertiesResult() { }
    public QueuePropertiesResult(int hashCode) : base(hashCode) { }

    public Dictionary<string, QueueProperties>? Queues { get; set; }
}
