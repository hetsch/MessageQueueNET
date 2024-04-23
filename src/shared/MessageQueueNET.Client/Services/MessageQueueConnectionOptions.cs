namespace MessageQueueNET.Client.Services;

public class MessageQueueConnectionOptions
{
    public string MessageQueueApiUrl { get; set; } = "";

    public string MessageQueueClientId { get; set; } = "";
    public string MessageQueueClientSecret { get; set; } = "";
}
