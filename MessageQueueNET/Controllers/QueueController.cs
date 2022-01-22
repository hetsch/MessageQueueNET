using MessageQueueNET.Client.Models;
using MessageQueueNET.Models;
using MessageQueueNET.Services;
using MessageQueueNET.Services.Abstraction;
using Microsoft.AspNetCore.Mvc;
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

                    result.RequireConfirmation = queue.Properties.ConfirmProcessingSeconds > 0;
                    if (result.RequireConfirmation == true)
                    {
                        result.ConfirmationPeriod = queue.Properties.ConfirmProcessingSeconds;
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
                            if (queue.Properties.ConfirmProcessingSeconds > 0)
                            {
                                _queues.AddToUnconfirmedMessage(queue.Name, item);
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
        public MessagesResult AllMessages(string id)
        {
            try
            {
                if (_queues.QueueExists(id))
                {
                    var queue = _queues.GetQueue(id);

                    var messages = queue
                        .Where(message => message.IsValid(queue))
                        .Select(message => new MessageResult()
                        {
                            Id = message.Id,
                            Value = message.Message
                        });

                    return new MessagesResult()
                    {
                        RequireConfirmation = queue.Properties.ConfirmProcessingSeconds > 0,
                        ConfirmationPeriod = queue.Properties.ConfirmProcessingSeconds > 0 ? queue.Properties.LifetimeSeconds : null,
                        Messages = messages
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
        public int Length(string id)
        {
            try
            {
                if (_queues.QueueExists(id))
                {
                    var queue = _queues.GetQueue(id);

                    return _queues.GetQueue(id)
                        .Where(item => item.IsValid(queue))
                        .Count();
                }
            }
            catch
            {

            }

            return 0;
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
        public bool Register(string id,
                             int? lifetimeSeconds = null,
                             int? itemLifetimeSeconds = null,
                             int? confirmProcessingSeconds = null,
                             bool? suspendEnqueue = null,
                             bool? suspendDequeue = null)
        {
            var queue = _queues.GetQueue(id);

            queue.Properties.LifetimeSeconds = lifetimeSeconds.HasValue ? lifetimeSeconds.Value : queue.Properties.LifetimeSeconds;
            queue.Properties.ItemLifetimeSeconds = itemLifetimeSeconds.HasValue ? itemLifetimeSeconds.Value : queue.Properties.ItemLifetimeSeconds;
            queue.Properties.ConfirmProcessingSeconds = confirmProcessingSeconds.HasValue ? confirmProcessingSeconds.Value : queue.Properties.ConfirmProcessingSeconds;
            queue.Properties.SuspendEnqueue = suspendEnqueue.HasValue ? suspendEnqueue.Value : queue.Properties.SuspendEnqueue;
            queue.Properties.SuspendDequeue = suspendDequeue.HasValue ? suspendDequeue.Value : queue.Properties.SuspendDequeue;

            _persist.PersistQueueProperties(queue);

            return true;
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
                ConfirmProcessingSeconds = queue.Properties.ConfirmProcessingSeconds,

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
