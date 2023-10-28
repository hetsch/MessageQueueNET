using MessageQueueNET.Client.Models;
using MessageQueueNET.Razor.Extensions;
using System.Collections.Concurrent;

namespace MessageQueueNET.Razor.Services;

internal class QueryQueueEventBus
{
    public ConcurrentDictionary<string, QueuePropertiesResult> QueuePropertiesResult = new();

    public event Func<string, QueuePropertiesResult, Task>? OnQueuePropertiesResultAsync;
    public Task FireQueuePropertiesResultAsync(string serverName, QueuePropertiesResult result)
    {
        QueuePropertiesResult[serverName] = result;
        return OnQueuePropertiesResultAsync?.FireAsync(serverName, result) ?? Task.CompletedTask;
    }

    public bool HasSubscribers => OnQueuePropertiesResultAsync?.GetInvocationList().Any() == true;
}
