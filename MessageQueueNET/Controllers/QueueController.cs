using MessageQueueNET.Models;
using MessageQueueNET.Services;
using MessageQueueNET.Services.Abstraction;
using Microsoft.AspNetCore.Http;
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
        async public Task<IEnumerable<string>> Dequeue(string id, int count = 1, bool register = false)
        {
            try
            {
                if (register == true || _queues.QueueExists(id))
                {
                    var queue = _queues.GetQueue(id);

                    List<string> messages = new List<string>();

                    while (messages.Count() < count)
                    {
                        if (queue.TryDequeue(out QueueItem item))
                        {
                            if (await _persist.RemoveQueueItem(id, item.Id) && item.IsValid(queue))
                            {
                                messages.Add(item.Message);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    return messages;
                }
            }
            catch
            {

            }

            return new string[0];
        }

        [HttpPut]
        [Route("enqueue/{id}")]
        async public Task<bool> Enqueue(string id, IEnumerable<string> messages)
        {
            try
            {
                var queue = _queues.GetQueue(id);

                foreach (var message in messages)
                {
                    var item = new QueueItem() { Message = message };
                    if (!await _persist.PersistQueueItem(id, item))
                        return false;

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
        public IEnumerable<string> AllMessages(string id)
        {
            try
            {
                if (_queues.QueueExists(id))
                {
                    var queue = _queues.GetQueue(id);

                    return queue
                        .Where(item => item.IsValid(queue))
                        .Select(item => item.Message);
                }
            }
            catch
            {
                
            }

            return new string[0];
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
        public bool Register(string id, int lifetimeSeconds = 0, int itemLifetimeSeconds = 0)
        {
            var queue = _queues.GetQueue(id);

            queue.Properties.LifetimeSeconds = lifetimeSeconds;
            queue.Properties.ItemLifetimeSeconds = itemLifetimeSeconds;

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
                ItemLifetimeSeconds = queue.Properties.ItemLifetimeSeconds
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
