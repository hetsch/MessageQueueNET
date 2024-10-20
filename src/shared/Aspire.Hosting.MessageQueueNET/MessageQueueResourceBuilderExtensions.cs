﻿using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

static public class MessageQueueResourceBuilderExtensions
{
    static public IResourceBuilder<MessageQueueResource> AddMessageQueueNET(
            this IDistributedApplicationBuilder builder,
            string name,
            int? httpPort = null,
            int? httpsPort = null,
            string? imageTag = null,
            string? bridgeNetwork = null,
            int maxRequestPollingSeconds = 5
        )
    {
        var resource = new MessageQueueResource(name);
        var resourceBuilder = builder.AddResource(resource)
                    .WithImage(MessageQueueContainerImageTags.Image)
                    .WithImageRegistry(MessageQueueContainerImageTags.Registry)
                    .WithImageTag(imageTag ?? MessageQueueContainerImageTags.Tag)
                    .WithContainerRuntimeArgs("--name", resource.ContainerName)
                    .WithEnvironment(e =>
                    {
                        e.EnvironmentVariables.Add("SWAGGERUI", "true");
                        e.EnvironmentVariables.Add("MESSAGEQUEUE__PERSIST__TYPE", "filesystem");
                        e.EnvironmentVariables.Add("MESSAGEQUEUE__PERSIST__ROOTPATH", "/home/app/messagequeue/persist");

                        if (maxRequestPollingSeconds > 0)
                        {
                            e.EnvironmentVariables.Add("MESSAGEQUEUE__MAXREQUESTPOLLINGSECONDS", maxRequestPollingSeconds.ToString());
                        }
                    })
                    .WithHttpEndpoint(
                          targetPort: resource.ContainerHttpPort,
                          port: httpPort,
                          name: MessageQueueResource.HttpEndpointName);

        if (!String.IsNullOrEmpty(bridgeNetwork))
        {
            resourceBuilder.WithContainerRuntimeArgs("--network", bridgeNetwork);
        }

        return resourceBuilder;
    }

    public static IResourceBuilder<MessageQueueResource> WithBindMountPersistance(
        this IResourceBuilder<MessageQueueResource> builder,
        string persistancePath = "{{local-app-data}}/messagequeue-aspire")
    {
        builder.WithBindMount(
                persistancePath.Replace("{{local-app-data}}", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)),
                "/home/app/messagequeue",
                isReadOnly: false
             );

        return builder;
    }

    public static IResourceBuilder<MessageQueueResource> WithVolumePersistance(
        this IResourceBuilder<MessageQueueResource> builder,
        string volumneName = "messagequeue-aspire")
    {
        builder.WithBindMount(
                volumneName,
                "/home/app/messagequeue",
                isReadOnly: false
             );

        return builder;
    }
}

internal static class MessageQueueContainerImageTags
{
    internal const string Registry = "docker.io";
    internal const string Image = "gstalt/messagequeue_net";
    internal const string Tag = "latest";
}