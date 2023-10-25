using MessageQueueNET.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MessageQueueNET.Razor.Services;

internal class QueryQueueBackgroundService : BackgroundService
{
    private readonly QueryQueueEventBus _eventBus;
    private readonly DashboardServiceOptions _options;

    public QueryQueueBackgroundService(QueryQueueEventBus eventBus,
                                       IOptions<DashboardServiceOptions> options)
    {
        _eventBus = eventBus;
        _options = options.Value;
    }

    async protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000);

            if (!_eventBus.HasSubscribers)
            {
                continue;
            }

            if (_options.Queues != null)
            {
                foreach (var queue in _options.Queues)
                {
                    try
                    {
                        var client = new QueueClient(queue.Url, queue.Filter);
                        var propertiesResult = await client.PropertiesAsync();

                        await _eventBus.FireQueuePropertiesResultAsync(queue.Name, propertiesResult);
                    }
                    catch (Exception /*ex*/)
                    {

                    }
                }
            }
        }
    }
}
