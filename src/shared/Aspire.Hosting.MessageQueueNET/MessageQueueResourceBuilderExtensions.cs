using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

static public class MessageQueueResourceBuilderExtensions
{
    static public MessageQueueResourceBuilder AddMessageQueueNET(
            this IDistributedApplicationBuilder builder,
            string name,
            int? httpPort = null,
            int? httpsPort = null,
            string? imageTag = null,
            string? bridgeNetwork = null
        )
    {
        var resource = new MessageQueueResource(name, bridgeNetwork);
        var resourceBuilder = builder.AddResource(resource)
                    .WithImage(MessageQueueContainerImageTags.Image)
                    .WithImageRegistry(MessageQueueContainerImageTags.Registry)
                    .WithImageTag(imageTag ?? MessageQueueContainerImageTags.Tag)
                    .WithHttpEndpoint(
                          targetPort: 8080,
                          port: httpPort,
                          name: MessageQueueResource.HttpEndpointName)
                      .WithHttpsEndpoint(
                          targetPort: 8443,
                          port: httpsPort,
                          name: MessageQueueResource.HttpsEndpointName);

        if (!String.IsNullOrEmpty(bridgeNetwork))
        {
            resourceBuilder.WithContainerRuntimeArgs("--network", bridgeNetwork);
        }

        return new MessageQueueResourceBuilder(builder, resourceBuilder);
    }
}

public class MessageQueueResourceBuilder(
        IDistributedApplicationBuilder appBuilder,
        IResourceBuilder<MessageQueueResource> resourceBuilder
    )
{
    internal IDistributedApplicationBuilder AppBuilder { get; } = appBuilder;
    internal IResourceBuilder<MessageQueueResource> ResourceBuilder { get; } = resourceBuilder;
}

internal static class MessageQueueContainerImageTags
{
    internal const string Registry = "docker.io";
    internal const string Image = "gstalt/messagequeue_net";
    internal const string Tag = "latest";
}