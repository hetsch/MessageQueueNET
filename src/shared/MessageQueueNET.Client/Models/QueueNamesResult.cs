using System;
using System.Collections.Generic;
using System.Text;

namespace MessageQueueNET.Client.Models;

public class QueueNamesResult : ApiResult
{
    public IEnumerable<string>? QueueNames { get; set; }
}
