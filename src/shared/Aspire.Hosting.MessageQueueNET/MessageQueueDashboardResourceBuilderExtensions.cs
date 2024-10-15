using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

static public class MessageQueueDashboardResourceBuilderExtensions
{
    static public MessageQueueDashboardResourceBuilder AddDashboardForMessageQueueNET(
            this IDistributedApplicationBuilder builder,
            string name,
            int? httpPort = null,
            int? httpsPort = null,
            string? imageTag = null,
            string? bridgeNetwork = null
        )
    {
        var resource = new MessageQueueDashboardResource(name, bridgeNetwork);
        var resourceBuilder = builder.AddResource(resource)
                    .WithImage(MessageQueueDashboardContainerImageTags.Image)
                    .WithImageRegistry(MessageQueueDashboardContainerImageTags.Registry)
                    .WithImageTag(imageTag ?? MessageQueueDashboardContainerImageTags.Tag)
                    .WithHttpEndpoint(
                          targetPort: resource.ContainerHttpPort,
                          port: httpPort,
                          name: MessageQueueResource.HttpEndpointName);

        if (!String.IsNullOrEmpty(bridgeNetwork))
        {
            resourceBuilder.WithContainerRuntimeArgs("--network", bridgeNetwork);
        }

        return new MessageQueueDashboardResourceBuilder(builder, resourceBuilder);
    }

    static public MessageQueueDashboardResourceBuilder ConnectToMessageQueue(
        this MessageQueueDashboardResourceBuilder builder,
        MessageQueueResourceBuilder messageQueueBuilder,
        string name,
        string filter = "*")
    {
        builder.ResourceBuilder.WithEnvironment(e =>
        {
            string url = $"http://{messageQueueBuilder.ResourceBuilder.Resource.HttpEndpoint.ContainerHost}:{messageQueueBuilder.ResourceBuilder.Resource.HttpEndpoint.Port}";

            if (!String.IsNullOrEmpty(builder.ResourceBuilder.Resource.BridgeNetwork) &&
                builder.ResourceBuilder.Resource.BridgeNetwork.Equals(
                    messageQueueBuilder.ResourceBuilder.Resource.BridgeNetwork
                )
               )
            {
                url = $"http://{messageQueueBuilder.ResourceBuilder.Resource.ContainerName}:{messageQueueBuilder.ResourceBuilder.Resource.ContainerHttpPort}";
            }

            e.EnvironmentVariables.Add($"DASHBOARD__QUEUES__{builder.QueueIndex}__NAME", name);
            e.EnvironmentVariables.Add($"DASHBOARD__QUEUES__{builder.QueueIndex}__URL", url);
            e.EnvironmentVariables.Add($"DASHBOARD__QUEUES__{builder.QueueIndex}__FILTER", filter);

            builder.QueueIndex++;
        });

        return builder;
    }

    static public MessageQueueDashboardResourceBuilder WithMaxPollingSeconds(
        this MessageQueueDashboardResourceBuilder builder,
        int maxPollingSeconds = 10)
    {
        builder.ResourceBuilder.WithEnvironment(e =>
        {
            e.EnvironmentVariables.Add($"DASHBOARD__MAXREQUESTPOLLINGSECONDS", maxPollingSeconds.ToString());
        });

        return builder;
    }
}

public class MessageQueueDashboardResourceBuilder(
        IDistributedApplicationBuilder appBuilder,
        IResourceBuilder<MessageQueueDashboardResource> resourceBuilder
    )
{
    internal IDistributedApplicationBuilder AppBuilder { get; } = appBuilder;
    internal IResourceBuilder<MessageQueueDashboardResource> ResourceBuilder { get; } = resourceBuilder;

    internal int QueueIndex = 0;
}

internal static class MessageQueueDashboardContainerImageTags
{
    internal const string Registry = "docker.io";
    internal const string Image = "gstalt/messagequeue_net_dashboard";
    internal const string Tag = "latest";
}