using MessageQueueNET.Core.Services.Abstraction;
using MessageQueueNET.Worker.Services;
using MessageQueueNET.Worker.Services.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MessageQueueNET.Worker.Extensions.DependencyInjection;

static public class ServiceCollectionExtensions
{
    static public IServiceCollection AddCommandLineWorker(this IServiceCollection services)
    {
        return services
            .AddTransient<ProcessRunnerService>()
            .AddTransient<IQueueProcessor, CommandLineWorker>();
    }
}
