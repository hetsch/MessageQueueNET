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
    }
}
