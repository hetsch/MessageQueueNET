using MessageQueueNET.Client.Models;
using MessageQueueNET.Models;
using MessageQueueNET.Services;
using MessageQueueNET.Services.Abstraction;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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
        [Route("dequeue/{id}")]
        async public Task<MessagesResult> Dequeue(string id, int count = 1, bool register = false)
        {
            var result = new MessagesResult();

            try
            {
                if (register == true || _queues.QueueExists(id))
                {
                    var queue = _queues.GetQueue(id);

                    result.RequireConfirmation = queue.Properties.ConfirmationPeriodSeconds > 0;
                    if (result.RequireConfirmation == true)
                    {
                        result.ConfirmationPeriod = queue.Properties.ConfirmationPeriodSeconds;
                    }

                    if (queue.Properties.SuspendDequeue == true)
                    {
                        return result;
                    }

                    List<MessageResult> items = new List<MessageResult>();

                    while (items.Count() < count)
                    {
                        if (queue.TryDequeue(out QueueItem item))
                        {
                            if (queue.Properties.ConfirmationPeriodSeconds > 0)
                            {
                                var unconfirmedItem = item.Clone();
                                if (!await _persist.PersistUnconfirmedQueueItem(id, unconfirmedItem))
                                {
                                    throw new Exception("Can't unable to persist unconfirmed queue item");
                                }

                                _queues.AddToUnconfirmedMessage(queue, unconfirmedItem);
                            }

                            if (await _persist.RemoveQueueItem(id, item.Id) && item.IsValid(queue))
                            {
                                items.Add(new MessageResult()
                                {
                                    Id = item.Id,
                                    Value = item.Message
                                });
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    result.Messages = items;
                }
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
        [Route("all/{id}")]
        public MessagesResult AllMessages(string id, int max = 0, bool unconfirmedOnly = false)
        {
            try
            {
                if (_queues.QueueExists(id))
                {
                    var queue = _queues.GetQueue(id);

                    IEnumerable<MessageResult> messages = null, unconfirmedMessages = null;

                    if (unconfirmedOnly == false)
                    {
                        messages = queue
                            .Where(message => message.IsValid(queue))
                            .Select(message => new MessageResult()
                            {
                                Id = message.Id,
                                Value = message.Message
                            });

                        if (max > 0)
                        {
                            messages = messages.Take(max);
                        }
                    }

                    unconfirmedMessages = _queues.UnconfirmedItems(queue);

                    if(max>0)
                    {
                        unconfirmedMessages = unconfirmedMessages?.Take(max);
                    }

                    return new MessagesResult()
                    {
                        RequireConfirmation = queue.Properties.ConfirmationPeriodSeconds > 0,
                        ConfirmationPeriod = queue.Properties.ConfirmationPeriodSeconds > 0 ? queue.Properties.ConfirmationPeriodSeconds : null,
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
        [Route("length/{id}")]
        public QueueLengthResult Length(string id)
        {
            try
            {
                if (_queues.QueueExists(id))
                {
                    var queue = _queues.GetQueue(id);

                    return new QueueLengthResult()
                    {
                        QueueLength = _queues.GetQueue(id)
                                             .Where(item => item.IsValid(queue))
                                             .Count(),
                        UnconfirmedItems = _queues.UnconfirmedMessagesCount(queue)
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
        public MessageQueueNET.Client.Models.QueueProperties Register(string id,
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
        public MessageQueueNET.Client.Models.QueueProperties QueueProperties(string id)
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
