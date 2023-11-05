using MessageQueueNET.Core.Services.Abstraction;
using MessageQueueNET.Worker.Services;
using MessageQueueNET.Worker.Services.Worker;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MessageQueueNET.Worker.Extensions.DependencyInjection;

static public class ServiceCollectionExtensions
{
    static public IServiceCollection AddCommandLineWorker(this IServiceCollection services,
                                                          Action<CommandLineWorkerOptions> commandLineWorkerOptions,
                                                          Action<CommandLineResultFileOutputWorkerOptions> fileOutputOptions)
    {
        return services
            .AddProccessRunnerService()
            .Configure(commandLineWorkerOptions)
            .Configure(fileOutputOptions)
            .AddTransient<IQueueProcessor, CommandLineWorker>()
            .AddTransient<IQueueProcessor, CommandLineResultConsoleOutputWorker>()
            .AddTransient<IQueueProcessor, CommandLineResultFileOutputWorker>();
    }

    static public IServiceCollection AddProccessRunnerService(this IServiceCollection services)
        => services.AddTransient<ProcessRunnerService>();
}
