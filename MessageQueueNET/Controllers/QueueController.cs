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
        async public Task<IEnumerable<string>> Dequeue(string id, int count = 1)
        {
            try
            {
                if (_queues.QueueExists(id))
                {
                    var queue = _queues.GetQueue(id);

                    List<string> messages = new List<string>();

                    for (int i = 0; i < count; i++)
                    {
                        if (queue.TryDequeue(out QueueItem item))
                        {
                            if (await _persist.RemoveQueueItem(id, item.Id))
                            {
                                messages.Add(item.Message);
                            }
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

                    return queue.Select(item => item.Message);
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
                    return _queues.GetQueue(id).Count;
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
        public bool Register(string id)
        {
            _queues.GetQueue(id);

            return true;
        }

        [HttpGet]
        [Route("queuenames")]
        public IEnumerable<string> QueueNames()
        {
            return _queues.QueueNames;
        }
    }
}
