using MessageQueueNET.Client;
using MessageQueueNET.Client.Models;
using MessageQueueNET.Razor.Extensions;
using Microsoft.Extensions.Options;

namespace MessageQueueNET.Razor.Services;

internal class DashboardService
{
    private readonly DashboardEventBusService _eventBus;
    private readonly DashboardServiceOptions _options;

    public DashboardService(DashboardEventBusService eventBus,
                            IOptions<DashboardServiceOptions> options)
    {
        _eventBus = eventBus;
        _options = options.Value;

        SelectedServerName = _options.Queues?.FirstOrDefault()?.Name ?? String.Empty;
    }

    public DashboardEventBusService EventBus => _eventBus;

    public IEnumerable<string> QueueServersNames =>
        _options.Queues?.Select(q => q.Name) ?? Array.Empty<string>();

    public string SelectedServerName { get; private set; }
    async public Task SetSelectedServerName(string serverName)
    {
        this.SelectedServerName = serverName;

        await _eventBus.FireQueueServerChangedAsync();
    }

    async public Task<MessagesResult> GetAllQueueMessages(string queueName)
    {
        var client = _options.GetQueueClient(SelectedServerName, queueName);
        var messages = await client.AllMessagesAsync();

        return messages;
    }

    async public Task<QueueProperties> GetQueueProperties(string queueName)
    {
        var client = _options.GetQueueClient(SelectedServerName, queueName);
        var propertiesResult = await client.PropertiesAsync();

        if(propertiesResult?.Queues == null ||
           propertiesResult.Queues.ContainsKey(queueName) == false)
        {
            throw new Exception($"Can't get properties for queue: {queueName}");
        }

        return propertiesResult.Queues[queueName];
    }

    async public Task<bool> SetQueueProperties(string queueName, QueueProperties queueProperties)
    {
        var client = _options.GetQueueClient(SelectedServerName, queueName); 

        await client.RegisterAsync(
            lifetimeSeconds: queueProperties.LifetimeSeconds,
            itemLifetimeSeconds: queueProperties.ItemLifetimeSeconds,
            confirmationPeriodSeconds: queueProperties.ConfirmationPeriodSeconds,
            suspendEnqueue: queueProperties.SuspendEnqueue,
            suspendDequeue: queueProperties.SuspendDequeue);

        return true;
    }
}
