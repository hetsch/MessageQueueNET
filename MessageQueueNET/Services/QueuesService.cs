using MessageQueueNET.Models;
using MessageQueueNET.Services.Abstraction;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        #region Handle Processing Confirmations

        public bool AddToUnconfirmedMessage(Queue queue, QueueItem queueItem)
        {
            try
            {
                if (queueItem != null)
                {
                    var unconfirmed = _unconfirmedItems.GetOrAdd(queue.Name, (key) => new ConcurrentDictionary<Guid, QueueItem>());
                    
                    return unconfirmed.TryAdd(queueItem.Id, queueItem);
                }
            }
            catch
            {

            }

            return false;
        }

        async public Task<bool> ReEnqueueUnconfirmedMessages(Queue queue, IQueuesPersistService persist)
        {
            try
            {
                if (_unconfirmedItems.TryGetValue(queue.Name, out ConcurrentDictionary<Guid, QueueItem> unconfirmedItems))
                {
                    foreach (var queueItem in unconfirmedItems.Values
                                                              .ToArray()
                                                              .Where(i => (DateTime.UtcNow - i.CreationDateUTC).TotalSeconds > queue.Properties.ConfirmationPeriodSeconds))
                    {
                        if (unconfirmedItems.TryRemove(queueItem.Id, out QueueItem item))
                        {
                            if (await persist.RemoveUnconfirmedQueueItem(queue.Name, item.Id) &&
                                await persist.PersistQueueItem(queue.Name, item))
                            {
                                queue.Enqueue(item);
                            }
                            else
                            {
                                unconfirmedItems.TryAdd(queueItem.Id, queueItem);
                            }
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

        public bool ConfirmDequeuedMessage(Queue queue, Guid messageId)
        {
            try
            {
                if (_unconfirmedItems.TryGetValue(queue.Name, out ConcurrentDictionary<Guid, QueueItem> unconfirmedItems))
                {
                    if (unconfirmedItems.TryGetValue(messageId, out QueueItem queueItem))
                    {
                        return unconfirmedItems.TryRemove(messageId, out queueItem);
                    }
                }
            }
            catch
            {

            }

            return false;
        }

        public int? UnconfirmedMessagesCount(Queue queue)
        {
            if (queue?.Properties == null || queue.Properties.ConfirmationPeriodSeconds <= 0)
            {
                return null;
            }

            if (_unconfirmedItems.TryGetValue(queue.Name, out ConcurrentDictionary<Guid, QueueItem> unconfirmedItems))
            {
                return unconfirmedItems.Count;
            }

            return 0;
        }

        public IEnumerable<Client.Models.MessageResult> UnconfirmedItems(Queue queue)
        {
            try
            {
                if (queue.Properties.ConfirmationPeriodSeconds >= 0 &&
                    _unconfirmedItems.TryGetValue(queue.Name, out ConcurrentDictionary<Guid, QueueItem> unconfirmedItems))
                {
                    var items = unconfirmedItems.Values
                                           .ToArray()
                                           .Select(i => new Client.Models.MessageResult()
                                           {
                                               Id = i.Id,
                                               Value = i.Message
                                           });

                    return items.Count() > 0 ? items : null;
                }
            }
            catch
            {

            }

            return null;
        }

        #endregion

        #region Restore

        public bool Restore(string queueName, 
                            QueueProperties properties, 
                            IEnumerable<QueueItem> items,
                            IEnumerable<QueueItem> unconfirmedItems)
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

                if (unconfirmedItems != null && unconfirmedItems.Count() > 0)
                {
                    var unconfirmed = _unconfirmedItems.GetOrAdd(queue.Name, (key) => new ConcurrentDictionary<Guid, QueueItem>());

                    foreach(var item in unconfirmedItems)
                    {
                        unconfirmed.TryAdd(item.Id, item);
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
