using MessageQueueNET.Client.Models;
using MessageQueueNET.Razor.Extensions;

namespace MessageQueueNET.Razor.Services;

internal class QueryQueueEventBus
{
    public event Func<string, QueuePropertiesResult, Task>? OnQueuePropertiesResultAsync;
    public Task FireQueuePropertiesResultAsync(string serverName, QueuePropertiesResult result)
        => OnQueuePropertiesResultAsync?.FireAsync(serverName, result) ?? Task.CompletedTask;

    public bool HasSubscribers => OnQueuePropertiesResultAsync?.GetInvocationList().Any() == true;
}
