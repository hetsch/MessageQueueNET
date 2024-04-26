using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MessageQueueNET.Models
{
    public class Queue : IEnumerable<QueueItem> //: ConcurrentQueue<QueueItem>
    {
        private QueueProperties _queueProperties;
        private ConcurrentQueue<QueueItem> _nestedQueue;

        public Queue(string name)
        {
            this.Name = name;

            this.LastAccessUTC = DateTime.UtcNow;
            this.LastModifiedUTC = DateTime.UtcNow;

            _nestedQueue = new ConcurrentQueue<QueueItem>();
            _queueProperties = new QueueProperties();
        }

        public string Name { get; }

        public DateTime LastAccessUTC { get; set; }
        public DateTime LastModifiedUTC { get; set; }

        public bool TryDelete(Guid id)
        {
            if (_nestedQueue.Any(i => i.Id == id))
            {
                _nestedQueue = new ConcurrentQueue<QueueItem>(_nestedQueue.Where(i => i.Id != id));
                return true;
            }

            return false;
        }

        public QueueProperties Properties
        {
            get { return _queueProperties; }
            internal set
            {
                _queueProperties = value ?? new QueueProperties();
            }
        }

        public bool TryDequeue(out QueueItem? item) => _nestedQueue.TryDequeue(out item);

        public void Enqueue(QueueItem item) => _nestedQueue.Enqueue(item);

        #region Nested

        public int Count => _nestedQueue.Count;
        public void Clear() => _nestedQueue.Clear();

        #region IEnumerable<QueueItem>

        public IEnumerator<QueueItem> GetEnumerator() => _nestedQueue.GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #endregion
    }
}
