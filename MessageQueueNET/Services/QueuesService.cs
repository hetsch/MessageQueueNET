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
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, QueueItem>> _unconfirmedItems;

        public QueuesService()
        {
            _queues = new ConcurrentDictionary<string, Queue>();
            _unconfirmedItems = new ConcurrentDictionary<string, ConcurrentDictionary<Guid, QueueItem>>();
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
                _unconfirmedItems.TryRemove(queueName, out ConcurrentDictionary<Guid, QueueItem> unconfirmedItems);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public IEnumerable<string> QueueNames => _queues.Keys.ToArray();

        public IEnumerable<Queue> Queues => _queues.Values.ToArray();

        #region Handle Processing Conformations

        public bool AddToUnconfirmedMessage(string queueName, QueueItem queueItem)
        {
            try
            {
                if (queueItem != null)
                {
                    var unconfirmed = _unconfirmedItems.GetOrAdd(queueName, (key) => new ConcurrentDictionary<Guid, QueueItem>());
                    
                    queueItem.ResetCreationDate();
                    return unconfirmed.TryAdd(queueItem.Id, queueItem);
                }
            }
            catch
            {
                
            }

            return false;
        }

        public bool ReEnqueueUnconfirmedMessages(Queue queue)
        {
            try
            {
                if(_unconfirmedItems.TryGetValue(queue.Name, out ConcurrentDictionary<Guid, QueueItem> unconfirmedItems))
                {
                    foreach (var queryItems in unconfirmedItems.Values
                                                               .ToArray()
                                                               .Where(i => (DateTime.UtcNow - i.CreationDateUTC).TotalSeconds > queue.Properties.ConfirmProcessingSeconds))
                    {
                        if(unconfirmedItems.TryRemove(queryItems.Id, out QueueItem item))
                        {
                            queue.Enqueue(item);
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Restore

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

        #endregion
    }
}
