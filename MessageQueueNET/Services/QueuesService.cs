using MessageQueueNET.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageQueueNET.Services
{
    public class QueuesService
    {
        private readonly ConcurrentDictionary<string, ConcurrentQueue<QueueItem>> _queues;

        public QueuesService()
        {
            _queues = new ConcurrentDictionary<string, ConcurrentQueue<QueueItem>>();
        }

        public ConcurrentQueue<QueueItem> GetQueue(string queueName)
        {
            return _queues.GetOrAdd(queueName, (key) => new ConcurrentQueue<QueueItem>());
        }

        public bool QueueExists(string queueName)
        {
            return _queues.ContainsKey(queueName);
        }

        public bool RemoveQueue(string queueName)
        {
            try
            {
                _queues.TryRemove(queueName, out ConcurrentQueue<QueueItem> queue);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public IEnumerable<string> QueueNames => _queues.Keys.ToArray();

        public bool Restore(string queueName, IEnumerable<QueueItem> items)
        {
            try
            {
                var queue = GetQueue(queueName);
                queue.Clear();

                if (items != null)
                {
                    foreach (var item in items.OrderBy(i => i.CreationDateUTC))
                    {
                        queue.Enqueue(item);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
