using System.Collections.Generic;

namespace MessageQueueNET.Client.Models;

public class QueueNamesResult : ApiResult
{
    public IEnumerable<string>? QueueNames { get; set; }
}
