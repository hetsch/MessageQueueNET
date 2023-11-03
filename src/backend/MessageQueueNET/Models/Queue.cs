using System;
using System.Collections.Concurrent;

namespace MessageQueueNET.Models
{
    public class Queue : ConcurrentQueue<QueueItem>
    {
        private QueueProperties _queueProperties;

        public Queue(string name)
        {
            this.Name = name;

            this.LastAccessUTC = DateTime.UtcNow;
            this.LastModifiedUTC = DateTime.UtcNow;

            _queueProperties = new QueueProperties();
        }

        public string Name { get; }

        public DateTime LastAccessUTC { get; set; }
        public DateTime LastModifiedUTC { get; set; }

        public QueueProperties Properties
        {
            get { return _queueProperties; }
            internal set
            {
                _queueProperties = value ?? new QueueProperties();
            }
        }
    }
}
