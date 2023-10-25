namespace MessageQueueNET.Razor.Services;

public class DashboardServiceOptions
{
    public IEnumerable<QueueModel>? Queues { get; set; }

    public class QueueModel
    {
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
        public string Filter { get; set; } = "*";
    }
}
