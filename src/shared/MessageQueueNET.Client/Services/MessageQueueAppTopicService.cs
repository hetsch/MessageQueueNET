using MessageQueueNET.Client.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageQueueNET.Client.Services;
public class MessageQueueAppTopicService
{
    private readonly MessageQueueAppTopicServiceOptions _options;
    private readonly string _queuePrefix;

    public MessageQueueAppTopicService(
            IOptions<MessageQueueAppTopicServiceOptions> options
        )
    {
        _options = options.Value;

        _queuePrefix = String.IsNullOrEmpty(_options.Namespace)
            ? _options.AppName
            : $"{_options.Namespace}.{_options.AppName}";
    }

    async public Task<bool> RegisterQueueAsync(int lifetime = 0, int itemLifetime = 0)
    {
        try
        {
            var ownQueueName = $"{_queuePrefix}{_options.InstanceId}";

            await ClientInstance(ownQueueName).RegisterAsync(lifetime, itemLifetime);

            return true;
        }
        catch { return false; }
    }

    async public Task<bool> EnqueueAsync(IEnumerable<string> messages, bool includeOwnQueue = false)
    {
        var client = ClientInstance($"{_queuePrefix}*");
        var queueProperties = await client.PropertiesAsync();

        if (queueProperties?.Queues is null)
        {
            return true;
        }

        var ownQueueName = $"{_queuePrefix}{_options.InstanceId}";

        foreach (var queueName in queueProperties.Queues.Keys)
        {
            if (includeOwnQueue == false && queueName == ownQueueName)
            {
                continue;
            }

            var queueClient = ClientInstance(queueName);
            await queueClient.EnqueueAsync(messages);
        }

        return true;
    }

    private QueueClient ClientInstance(string queueNameWithNamespace)
         => new QueueClient(
                    _options.ToConnection(),
                    queueNameWithNamespace
                );
}
