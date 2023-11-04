using System;

namespace MessageQueueNET.Client.Models
{
    public class QueueProperties
    {
        public DateTime LastAccessUTC { get; set; }
        public DateTime LastModifiedUTC { get; set; }

        public int Length { get; set; }
        public int? UnconfirmedItems { get; set; }
        public int? DequeuingClientsCount { get; set; }

        public int LifetimeSeconds { get; set; }
        public int ItemLifetimeSeconds { get; set; }

        public int ConfirmationPeriodSeconds { get; set; }
        public int? MaxUnconfirmedItems { get; set; }

        public bool SuspendEnqueue { get; set; }
        public bool SuspendDequeue { get; set; }
    }
}
