using MessageQueueNET.Client;
using MessageQueueNET.Client.Models;
using MessageQueueNET.Client.Services;
using MessageQueueNET.Razor.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MessageQueueNET.Razor.Services;

internal class QueryQueueBackgroundService : BackgroundService
{
    private readonly QueryQueueEventBus _eventBus;
    private readonly MessageQueueClientService _clientService;
    private readonly DashboardServiceOptions _options;

    public QueryQueueBackgroundService(QueryQueueEventBus eventBus,
                                       MessageQueueClientService clientService,
                                       IOptions<DashboardServiceOptions> options)
    {
        _eventBus = eventBus;
        _clientService = clientService;
        _options = options.Value;
    }

    async protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_options.Queues != null)
        {
            List<Task> tasks = new List<Task>();

            foreach (var queueModel in _options.Queues)
            {
                tasks.Add(Task.Factory.StartNew(
                    async (state) =>
                    {
                        var queue = (DashboardServiceOptions.QueueModel)state!;

                        while (!stoppingToken.IsCancellationRequested)
                        {
                            try
                            {
                                await foreach (var propertiesResult in _clientService.GetNextQueueProperties(new MessageQueueConnection(queue.Url),
                                                                                                             queue.Filter,
                                                                                                             stoppingToken,
                                                                                                             silentAccess: true))
                                {
                                    await TaskExt.WaitUntil(() => _eventBus.HasSubscribers);

                                    await _eventBus.FireQueuePropertiesResultAsync(queue.Name, propertiesResult);
                                }
                            }
                            catch (Exception /*ex*/)
                            {
                                await Task.Delay(1000);
                            }
                        }
                    },
                    queueModel,
                    stoppingToken));
            }

            await TaskExt.WaitForCancellation(stoppingToken);
        }
    }

    async protected /*override*/ Task ExecuteAsync_old(CancellationToken stoppingToken)
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
                        var client = new QueueClient(new MessageQueueConnection(queue.Url), queue.Filter);
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
