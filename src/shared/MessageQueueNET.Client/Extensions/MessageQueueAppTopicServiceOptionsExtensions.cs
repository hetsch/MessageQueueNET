using MessageQueueNET.Client.Services;
using System;

namespace MessageQueueNET.Client.Extensions;
static public class MessageQueueAppTopicServiceOptionsExtensions
{
    static public string ToQueueName(this MessageQueueAppTopicServiceOptions options)
        => String.IsNullOrEmpty(options.Namespace)
                ? $"{options.AppName}{options.InstanceId}"
                : $"{options.Namespace}.{options.AppName}{options.InstanceId}";

    static public string ToTopicQueuesFilter(this MessageQueueAppTopicServiceOptions options)
        => String.IsNullOrEmpty(options.Namespace)
                ? $"{options.AppName}*"
                : $"{options.Namespace}.{options.AppName}*";

}
