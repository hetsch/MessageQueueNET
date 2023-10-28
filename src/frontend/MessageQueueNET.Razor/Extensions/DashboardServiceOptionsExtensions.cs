using MessageQueueNET.Client;
using MessageQueueNET.Client.Models;
using MessageQueueNET.Client.Services;
using MessageQueueNET.Razor.Services;

namespace MessageQueueNET.Razor.Extensions;

static internal class DashboardServiceOptionsExtensions
{
    static public DashboardServiceOptions.QueueModel QueueModelByName(this DashboardServiceOptions options, string modelName)
        => options.Queues?.First(q => q.Name == modelName) ?? throw new Exception($"Unknown server: {modelName}");

    async static public ValueTask<QueueClient> GetQueueClient(this DashboardServiceOptions options,
                                                              MessageQueueClientService clientService,
                                                              string modelName, 
                                                              string queueName)
        => await clientService.CreateClient(
            new MessageQueueConnection(options.QueueModelByName(modelName).Url), queueName);

    static public string NewQueueNamePattern(this DashboardServiceOptions options, string modelName)
    {
        var queueModel = options.QueueModelByName(modelName);

        return queueModel switch
        {
            null => string.Empty,
            DashboardServiceOptions.QueueModel m when (m.Filter.Count(c => c == '*') == 1 && m.Filter.EndsWith("*")) => m.Filter,
            _ => string.Empty
        };
    }

    static public string DeleteQueueNamePattern(this DashboardServiceOptions options, string modelName)
    {
        var queueModel = options.QueueModelByName(modelName);

        return queueModel switch
        {
            DashboardServiceOptions.QueueModel m => m.Filter,
            _ => String.Empty
        };
    }
}
