using MessageQueueNET.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MessageQueueNET.Services
{
    public class QueuesService
    {
        private readonly ConcurrentDictionary<string, Queue> _queues;

        public QueuesService()
        {
            _queues = new ConcurrentDictionary<string, Queue>();
        }

        public Queue GetQueue(string queueName, bool forAccess = true)
        {
            var queue = _queues.GetOrAdd(queueName, (key) => new Queue(queueName));

            if (forAccess == true)
            {
                queue.LastAccessUTC = DateTime.UtcNow;
            }

            return queue;
        }

        public bool QueueExists(string queueName)
        {
            return _queues.ContainsKey(queueName);
        }

        public bool RemoveQueue(string queueName)
        {
            try
            {
                _queues.TryRemove(queueName, out Queue queue);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public IEnumerable<string> QueueNames => _queues.Keys.ToArray();

        public IEnumerable<Queue> Queues => _queues.Values.ToArray();

        public bool Restore(string queueName, QueueProperties properties, IEnumerable<QueueItem> items)
        {
            try
            {
                var queue = GetQueue(queueName);
                queue.Properties = properties;
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
