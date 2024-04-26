using MessageQueueNET.Client.Extensions.DependencyInjetion;
using MessageQueueNET.Processor;
using MessageQueueNET.Worker.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var arguments = new CommandLine().Parse(args);
if (!arguments.HasValue)
{
    return;
}

var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services
                    .AddCommandLineWorker(
                        commandLineWorkerOptions =>
                        {
                            commandLineWorkerOptions.CommandFilters = arguments.Value.commandFilter
                                                                                     .Split(',')
                                                                                     .Select(c => c.Trim())
                                                                                     .Where(c => !string.IsNullOrEmpty(c))
                                                                                     .ToArray();
                        },
                        fileOutputOptions =>
                        {
                            fileOutputOptions.RootPath = arguments.Value.outputPath;
                        })
                    .AddMessageQueueClientService()
                    .AddQueueProcessorWatcher(
                        config =>
                        {
                            config.MessageQueueApiUrl = arguments.Value.apiUrl;
                            config.QueueNameFilter = arguments.Value.filter;
                        });
            })
            .ConfigureLogging(logging =>
            {
                logging.AddFilter("System", LogLevel.Warning);
                logging.AddFilter("Microsoft", LogLevel.Warning);

                logging.AddFilter((provider, category, logLevel) =>
                {
                    if (category?.StartsWith("System.") == false
                        && category.StartsWith("Microsoft.") == false)
                    {
                        return logLevel >= LogLevel.Information;
                    }

                    return false;
                });
                //config.SetMinimumLevel(LogLevel.Warning);
            })
            .Build();

Console.WriteLine("Press Ctrl+C to exit...");

host.Run();

