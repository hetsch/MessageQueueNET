using MessageQueueNET.Services.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageQueueNET.Services
{
    public class RestorePersistedQueuesService
    {
        private readonly QueuesService _queues;
        private readonly IQueuesPersistService _persist;

        public RestorePersistedQueuesService(QueuesService queues,
                                             IQueuesPersistService persist)
        {
            _queues = queues;
            _persist = persist;
        }

        async public Task<bool> Restore()
        {
            try
            {
                foreach (var queueName in await _persist.QueueNames())
                {
                    _queues.Restore(queueName,
                                    await _persist.GetQueueProperties(queueName),
                                    await _persist.GetAllItems(queueName));
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
