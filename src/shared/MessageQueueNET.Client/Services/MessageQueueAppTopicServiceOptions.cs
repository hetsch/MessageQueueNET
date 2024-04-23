using System;

namespace MessageQueueNET.Client.Services;

public class MessageQueueAppTopicServiceOptions : MessageQueueConnectionOptions
{
    public MessageQueueAppTopicServiceOptions()
    {
        InstanceId = Guid.NewGuid().ToString("N").ToLowerInvariant();
    }


    public bool TryRegisterQueues { get; set; } = false;
    public int? LifetimeSeconds { get; set; }

    public string Namespace { get; set; } = "";
    public string AppName { get; set; } = "";
    internal string InstanceId { get; } = "";
}
