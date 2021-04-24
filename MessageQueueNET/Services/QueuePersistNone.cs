using MessageQueueNET.Models;
using MessageQueueNET.Services.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageQueueNET.Services
{
    public class QueuePersistNone : IQueuesPersistService
    {
        public Task<IEnumerable<QueueItem>> GetAllItems(string queueName)
        {
            return Task.FromResult<IEnumerable<QueueItem>>(new QueueItem[0]);
        }

        public Task<bool> PersistQueueItem(string queueName, QueueItem item)
        {
            return Task.FromResult(true);
        }

        public Task<IEnumerable<string>> QueueNames()
        {
            return Task.FromResult<IEnumerable<string>>(new string[0]);
        }

        public Task<bool> RemoveQueue(string queueName)
        {
            return Task.FromResult(true);
        }

        public Task<bool> RemoveQueueItem(string queueName, Guid itemId)
        {
            return Task.FromResult(false);
        }
    }
}
