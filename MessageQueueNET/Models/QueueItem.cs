using System;

namespace MessageQueueNET.Models
{
    public class QueueItem
    {
        public QueueItem()
        {
            this.Id = Guid.NewGuid();
            ResetCreationDate();
        }

        public Guid Id { get; set; }
        public DateTime CreationDateUTC { get; set; }
        public string Message { get; set; }

        private void ResetCreationDate()
        {
            this.CreationDateUTC = DateTime.UtcNow;
        }

        public QueueItem Clone()
        {
            return new QueueItem()
            {
                Id = this.Id,
                Message = this.Message
            };
        }

        public bool IsValid(Queue queue)
        {
            if (queue.Properties.ItemLifetimeSeconds > 0 && 
                (DateTime.UtcNow - this.CreationDateUTC).TotalSeconds > queue.Properties.ItemLifetimeSeconds)
            {
                return false;
            }

            return true;
        }
    }
}
