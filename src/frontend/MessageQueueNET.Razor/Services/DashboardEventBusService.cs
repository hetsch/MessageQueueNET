using MessageQueueNET.Razor.Extensions;

namespace MessageQueueNET.Razor.Services;

internal class DashboardEventBusService
{
    public event Func<Task>? OnQueueServerChangedAsync;
    public Task FireQueueServerChangedAsync()
        => OnQueueServerChangedAsync?.FireAsync() ?? Task.CompletedTask;
}
