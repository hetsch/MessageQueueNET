using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageQueueNET.Services
{
    public class QueuesService
    {
        private readonly ConcurrentDictionary<string, ConcurrentQueue<string>> _queues;

        public QueuesService()
        {
            _queues = new ConcurrentDictionary<string, ConcurrentQueue<string>>();
        }

        public ConcurrentQueue<string> GetQueue(string queueName)
        {
            return _queues.GetOrAdd(queueName, (key) => new ConcurrentQueue<string>());
        }

        public bool RemoveQueue(string queueName)
        {
            try
            {
                _queues.TryRemove(queueName, out ConcurrentQueue<string> queue);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
