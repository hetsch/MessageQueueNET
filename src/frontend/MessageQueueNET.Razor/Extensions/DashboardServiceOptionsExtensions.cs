using MessageQueueNET.Client;
using MessageQueueNET.Razor.Services;

namespace MessageQueueNET.Razor.Extensions;

static internal class DashboardServiceOptionsExtensions
{
    static public DashboardServiceOptions.QueueModel QueueModelByName(this DashboardServiceOptions options, string modelName)
        => options.Queues?.First(q => q.Name == modelName) ?? throw new Exception($"Unknown server: {modelName}");

    static public QueueClient GetQueueClient(this DashboardServiceOptions options, string modelName, string queueName)
        => new QueueClient(options.QueueModelByName(modelName).Url, queueName); 
}
