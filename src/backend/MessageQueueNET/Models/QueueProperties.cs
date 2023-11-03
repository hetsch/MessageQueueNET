namespace MessageQueueNET.Models
{
    public class QueueProperties
    {
        public QueueProperties()
        {
            this.LifetimeSeconds = 0;
            this.ItemLifetimeSeconds = 0;
        }

        public int LifetimeSeconds { get; set; }
        public int ItemLifetimeSeconds { get; set; }

        public int ConfirmationPeriodSeconds { get; set; }
        public int MaxUnconfiredItems { get; set; }

        public bool SuspendEnqueue { get; set; }
        public bool SuspendDequeue { get; set; }
    }
}
