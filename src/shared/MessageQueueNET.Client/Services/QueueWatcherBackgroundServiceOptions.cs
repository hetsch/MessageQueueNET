namespace MessageQueueNET.Client.Services;
public class QueueWatcherBackgroundServiceOptions : MessageQueueConnectionOptions
{
    public string QueueNameFilter { get; set; } = "*";
    public bool TryRegisterQueues { get; set; } = false;
}
