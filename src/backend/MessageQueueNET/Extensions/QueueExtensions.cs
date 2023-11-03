using MessageQueueNET.Models;
using System;
using System.Collections.Generic;

namespace MessageQueueNET.Extensions;

static public class QueueExtensions
{
    static public void SetModified(this Queue queue)
    {
        queue.LastModifiedUTC = DateTime.UtcNow;
    }

    static public void SetModified(this IEnumerable<Queue> queues)
    {
        foreach(var queue in queues)
        {
            queue.SetModified();
        }
    }
}
