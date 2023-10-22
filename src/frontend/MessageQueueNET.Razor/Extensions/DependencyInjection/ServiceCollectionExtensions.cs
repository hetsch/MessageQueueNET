using MessageQueueNET.Razor.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MessageQueueNET.Razor.Extensions.DependencyInjection;

static public class ServiceCollectionExtensions
{
    static public IServiceCollection AddDashboardService(this IServiceCollection services,
                                                         Action<DashboardServiceOptions> setupConfig)
    {
        return services
            .Configure(setupConfig)
            .AddTransient<DashboardService>();
    }
}
