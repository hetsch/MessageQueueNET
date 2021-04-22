using MessageQueueNET.Services;
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

        public QueueController(QueuesService queues)
        {
            _queues = queues;
        }

        [HttpGet]
        [Route("dequeue/{id}")]
        public IEnumerable<string> Dequeue(string id, int count = 1)
        {
            try
            {
                var queue = _queues.GetQueue(id);

                List<string> messages = new List<string>();

                for (int i = 0; i < count; i++)
                {
                    if (queue.TryDequeue(out string message))
                    {
                        messages.Add(message);
                    }
                }

                return messages;
            }
            catch
            {
                return new string[0];
            }
        }

        [HttpPut]
        [Route("enqueue/{id}")]
        public bool Enqueue(string id, IEnumerable<string> messages)
        {
            try
            {
                var queue = _queues.GetQueue(id);

                foreach (var message in messages)
                {
                    queue.Enqueue(message);
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
                var queue = _queues.GetQueue(id);

                return queue.Select(v => v);
            }
            catch
            {
                return new string[0];
            }
        }

        [HttpGet]
        [Route("lenght/{id}")]
        public int Length(string id)
        {
            try
            {
                return _queues.GetQueue(id).Count;
            }
            catch
            {
                return 0;
            }
        }

        [HttpGet]
        [Route("remove/{id}")]
        public bool Remove(string id)
        {
            try
            {
                return _queues.RemoveQueue(id);
            }
            catch
            {
                return false;
            }
        }
    }
}
