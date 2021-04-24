using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageQueueNET.Models
{
    public class QueueItem
    {
        public QueueItem()
        {
            this.Id = Guid.NewGuid();
            this.CreationDateUTC = DateTime.UtcNow;
        }

        public Guid Id { get; set; }
        public DateTime CreationDateUTC { get; set; }
        public string Message { get; set; }
    }
}
