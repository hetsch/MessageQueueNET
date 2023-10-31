using MessageQueueNET.Processor;
using MessageQueueNET.Client.Extensions.DependencyInjetion;
using Microsoft.Extensions.Hosting;

var arguments = new CommandLine().Parse(args);
if (!arguments.HasValue)
{
    return;
}

var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddQueueWatcher(config =>
                {
                    config.MessageQueueApiUrl = arguments.Value.apiUrl;
                    config.QueueNameFilter = arguments.Value.filter;
                });
            })
            .Build();

host.Run();