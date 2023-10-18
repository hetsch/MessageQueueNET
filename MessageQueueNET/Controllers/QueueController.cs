using MessageQueueNET.Client.Models;
using MessageQueueNET.Extensions;
using MessageQueueNET.Models;
using MessageQueueNET.Services;
using MessageQueueNET.Services.Abstraction;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace MessageQueueNET.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class QueueController : ControllerBase
    {
        private readonly QueuesService _queues;
        private readonly IQueuesPersistService _persist;

        public QueueController(QueuesService queues,
                               IQueuesPersistService persist)
        {
            _queues = queues;
            _persist = persist;
        }

        [HttpGet]
        [Route("dequeue/{idPattern}")]
        async public Task<MessagesResult> Dequeue(string idPattern, int count = 1, bool register = false)
        {
            var result = new MessagesResult();

            try
            {
                if (register == true
                    && idPattern.IsPattern() == false
                    && _queues.QueueExists(idPattern) == false)
                {
                    _queues.GetQueue(idPattern);  // register Queue
                }

                var queryQueues = _queues.GetQueues(idPattern);
                if (queryQueues.Any() == false)
                {
                    return result;
                }

                List<MessageResult> items = new List<MessageResult>();

                while (items.Count() < count)
                {
                    var queue = queryQueues.QueueWithOldestDequeueAbleItem();
                    if (queue == null)
                    {
                        break;
                    }

                    if (queue.TryDequeue(out QueueItem? item))
                    {
                        if (queue.Properties.ConfirmationPeriodSeconds > 0)
                        {
                            var unconfirmedItem = item.Clone();
                            if (!await _persist.PersistUnconfirmedQueueItem(queue.Name, unconfirmedItem))
                            {
                                throw new Exception("Can't unable to persist unconfirmed queue item");
                            }

                            _queues.AddToUnconfirmedMessage(queue, unconfirmedItem);
                        }

                        if (await _persist.RemoveQueueItem(queue.Name, item.Id) && item.IsValid(queue))
                        {
                            items.Add(new MessageResult()
                            {
                                Queue = queue.Name,
                                Id = item.Id,
                                Value = item.Message,
                                RequireConfirmation = queue.Properties.ConfirmationPeriodSeconds > 0 ? true : null,
                                ConfirmationPeriod = queue.Properties.ConfirmationPeriodSeconds > 0 ? queue.Properties.ConfirmationPeriodSeconds : null,
                            });
                        }
                    }
                }

                result.Messages = items;
            }
            catch
            {

            }

            return result;
        }

        [HttpGet]
        [Route("confirmdequeue/{id}")]
        async public Task<bool> ConfirmDequeue(string id, Guid messageId)
        {
            try
            {
                var queue = _queues.GetQueue(id);

                if (queue.Properties.ConfirmationPeriodSeconds > 0)
                {
                    if (await _persist.RemoveUnconfirmedQueueItem(queue.Name, messageId))
                    {
                        return _queues.ConfirmDequeuedMessage(queue, messageId);
                    }
                }
            }
            catch
            {

            }

            return false;
        }

        [HttpPut]
        [Route("enqueue/{id}")]
        async public Task<bool> Enqueue(string id, IEnumerable<string> messages)
        {
            try
            {
                var queue = _queues.GetQueue(id);

                if (queue.Properties.SuspendEnqueue == true)
                {
                    return false;
                }

                foreach (var message in messages)
                {
                    var item = new QueueItem() { Message = message };
                    if (!await _persist.PersistQueueItem(id, item))
                    {
                        return false;
                    }

                    queue.Enqueue(item);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        [HttpGet]
        [Route("all/{idPattern}")]
        public MessagesResult AllMessages(string idPattern, int max = 0, bool unconfirmedOnly = false)
        {
            try
            {
                if (_queues.AnyQueueExists(idPattern))
                {
                    var messages = new List<MessageResult>();
                    var unconfirmedMessages = new List<MessageResult>();

                    foreach (var queue in _queues.GetQueues(idPattern))
                    {
                        if (unconfirmedOnly == false)
                        {
                            messages.AddRange(queue
                                .Where(message => message.IsValid(queue))
                                .Select(message => new MessageResult()
                                {
                                    Queue = queue.Name,
                                    Id = message.Id,
                                    Value = message.Message,
                                    RequireConfirmation = queue.Properties.ConfirmationPeriodSeconds > 0 ? true : null,
                                    ConfirmationPeriod = queue.Properties.ConfirmationPeriodSeconds > 0 ? queue.Properties.ConfirmationPeriodSeconds : null,
                                }));

                            if (max > 0)
                            {
                                messages = messages.Take(max).ToList();
                            }
                        }

                        unconfirmedMessages.AddRange(_queues.UnconfirmedItems(queue) ?? Array.Empty<MessageResult>());

                        if (max > 0)
                        {
                            unconfirmedMessages = unconfirmedMessages.Take(max).ToList();
                        }
                    }

                    return new MessagesResult()
                    {
                        Messages = messages,
                        UnconfirmedMessages = unconfirmedMessages
                    };
                }
            }
            catch
            {

            }

            return new MessagesResult();
        }

        [HttpGet]
        [Route("length/{idPattern}")]
        public QueueLengthResult Length(string idPattern)
        {
            try
            {
                if (_queues.AnyQueueExists(idPattern))
                {
                    var queueLengthItems = new Dictionary<string, QueueLengthItem>(); 

                    foreach (var queue in _queues.GetQueues(idPattern))
                    {
                        queueLengthItems[queue.Name] = new QueueLengthItem()
                        {
                            QueueLength = queue.Where(item => item.IsValid(queue))
                                               .Count(),
                            UnconfirmedItems = _queues.UnconfirmedMessagesCount(queue)
                        };
                    }

                    return new QueueLengthResult()
                    {
                        Queues = queueLengthItems
                    };
                }
            }
            catch
            {

            }

            return new QueueLengthResult();
        }

        [HttpGet]
        [Route("remove/{id}")]
        async public Task<bool> Remove(string id)
        {
            try
            {
                if (await _persist.RemoveQueue(id))
                {
                    return _queues.RemoveQueue(id);
                }
            }
            catch
            {

            }

            return false;
        }

        [HttpGet]
        [Route("register/{id}")]
        public Client.Models.QueueProperties Register(string id,
                             int? lifetimeSeconds = null,
                             int? itemLifetimeSeconds = null,
                             int? confirmationPeriodSeconds = null,
                             bool? suspendEnqueue = null,
                             bool? suspendDequeue = null)
        {
            var queue = _queues.GetQueue(id);

            queue.Properties.LifetimeSeconds = lifetimeSeconds.HasValue ? lifetimeSeconds.Value : queue.Properties.LifetimeSeconds;
            queue.Properties.ItemLifetimeSeconds = itemLifetimeSeconds.HasValue ? itemLifetimeSeconds.Value : queue.Properties.ItemLifetimeSeconds;
            queue.Properties.ConfirmationPeriodSeconds = confirmationPeriodSeconds.HasValue ? confirmationPeriodSeconds.Value : queue.Properties.ConfirmationPeriodSeconds;
            queue.Properties.SuspendEnqueue = suspendEnqueue.HasValue ? suspendEnqueue.Value : queue.Properties.SuspendEnqueue;
            queue.Properties.SuspendDequeue = suspendDequeue.HasValue ? suspendDequeue.Value : queue.Properties.SuspendDequeue;

            _persist.PersistQueueProperties(queue);

            return QueueProperties(id);
        }

        [HttpGet]
        [Route("properties/{id}")]
        public Client.Models.QueueProperties QueueProperties(string id)
        {
            var queue = _queues.GetQueue(id);

            return new Client.Models.QueueProperties()
            {
                LastAccessUTC = queue.LastAccessUTC,
                Length = queue.Where(item => item.IsValid(queue))
                              .Count(),

                LifetimeSeconds = queue.Properties.LifetimeSeconds,
                ItemLifetimeSeconds = queue.Properties.ItemLifetimeSeconds,
                ConfirmationPeriodSeconds = queue.Properties.ConfirmationPeriodSeconds,

                SuspendDequeue = queue.Properties.SuspendDequeue,
                SuspendEnqueue = queue.Properties.SuspendEnqueue,
            };
        }

        [HttpGet]
        [Route("queuenames")]
        public IEnumerable<string> QueueNames()
        {
            return _queues.QueueNames;
        }
    }
}
