namespace MessageQueueNET.Client;

public static class MQHeaders
{
    public const string HashCode = "X-MQ-HashCode";
    public const string ClientId = "X-MQ-ClientId";
    public const string MaxPollingSeconds = "X-MQ-MaxPollingSeconds";
    // used by dashboard, to not touch the LastAccessUTC when getting 
    // Queue Properties
    public const string SilentAccess = "X-MQ-SilentAccess";
}
