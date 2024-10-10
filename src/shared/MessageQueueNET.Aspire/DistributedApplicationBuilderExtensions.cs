using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace MessageQueueNET.Aspire;

static public class DistributedApplicationBuilderExtensions
{
    static public IResourceBuilder<ContainerResource> AddMessageQueueNET(
                this IDistributedApplicationBuilder builder,
                int port = 9091,
                int maxRequestPollingSeconds = 5,
                string mountSource = ""
        )
    {
        var resource = builder.AddContainer("message-queue", "gstalt/messagequeue_net", "latest")
            .WithHttpEndpoint(port, 8080)
            .WithEnvironment(e =>
            {
                e.EnvironmentVariables.Add("SWAGGERUI", "true");
                e.EnvironmentVariables.Add("MESSAGEQUEUE__PERSIST__TYPE", "filesystem");
                e.EnvironmentVariables.Add("MESSAGEQUEUE__PERSIST__ROOTPATH", "/etc/messagequeue/persist");

                if (maxRequestPollingSeconds > 0)
                {
                    e.EnvironmentVariables.Add("MESSAGEQUEUE__MAXREQUESTPOLLINGSECONDS", maxRequestPollingSeconds.ToString());
                }
            });

        if(!string.IsNullOrEmpty(mountSource))
        {
            resource.WithBindMount(mountSource, "/etc/messagequeue");
        }

        return resource;
    }

    static public IResourceBuilder<ContainerResource> AddMessageQueueNETDashboard(
                this IDistributedApplicationBuilder builder,
                int port = 9090,
                string messageQueueUrl = "http://{{localhost}}:9091"
        )
    {
        var resource = builder.AddContainer("message-queue-dashboard", "gstalt/messagequeue_net_dashboard", "latest")
            .WithHttpEndpoint(port, 8080)
            .WithEnvironment(e =>
            {
                e.EnvironmentVariables.Add("DASHBOARD__QUEUES__0__NAME", "queues");
                e.EnvironmentVariables.Add("DASHBOARD__QUEUES__0__URL", messageQueueUrl.Replace("{{localhost}}", "docker.for.mac.localhost"));
            });

        return resource;
    }
}
