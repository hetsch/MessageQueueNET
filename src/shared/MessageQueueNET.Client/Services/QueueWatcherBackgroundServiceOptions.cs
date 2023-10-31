namespace MessageQueueNET.Client.Services;
public class QueueWatcherBackgroundServiceOptions
{
    public string MessageQueueApiUrl { get; set; } = "";
    public string QueueNameFilter { get; set; } = "*";
}
