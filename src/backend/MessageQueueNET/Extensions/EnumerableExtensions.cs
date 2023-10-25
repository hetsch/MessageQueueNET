using MessageQueueNET.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MessageQueueNET.Extensions
{
    static public class EnumerableExtensions
    {
        static public Queue? QueueWithOldestDequeueAbleItem(this IEnumerable<Queue> queues)
        {
            Queue? result = null;
            DateTime? oldestItemTime = null;

            foreach (var queue in queues)
            {
                if (queue.Properties.SuspendDequeue == true || queue.Count() == 0)
                {
                    continue;
                }

                var oldestQueueItemTime = queue.Select(i => i.CreationDateUTC).Min();
                if (oldestItemTime == null || oldestQueueItemTime < oldestItemTime)
                {
                    oldestItemTime = oldestQueueItemTime;
                    result = queue;
                }
            }

            return result;
        }
    }
}
