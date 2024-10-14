using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

static public class MessageQueueDashboardResourceBuilderExtensions
{
}

public class MessageQueueResourceDashboardBuilder(
        IDistributedApplicationBuilder appBuilder,
        IResourceBuilder<MessageQueueDashboardResource> resourceBuilder
    )
{
    internal IDistributedApplicationBuilder AppBuilder { get; } = appBuilder;
    internal IResourceBuilder<MessageQueueDashboardResource> ResourceBuilder { get; } = resourceBuilder;
}

internal static class MessageQueueDashboardContainerImageTags
{
    internal const string Registry = "docker.io";
    internal const string Image = "gstalt/messagequeue_net_dashboard";
    internal const string Tag = "latest";
}