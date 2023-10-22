using Microsoft.Extensions.Options;

namespace MessageQueueNET.Razor.Services;

public class DashboardService
{
    private readonly DashboardServiceOptions _options;

    public DashboardService(IOptions<DashboardServiceOptions> options)
    {
        _options = options.Value;
    }
}
