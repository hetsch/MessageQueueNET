using System;
using System.Collections.Generic;
using System.Text;

namespace MessageQueueNET.Client.Models
{
    public class QueueProperties
    {
        public DateTime LastAccessUTC { get; set; }
        public int Length { get; set; }
        public int LifetimeSeconds { get; set; }
        public int ItemLifetimeSeconds { get; set; }

        public bool SuspendEnqueue { get; set; }
        public bool SuspendDequeue { get; set; }
    }
}
