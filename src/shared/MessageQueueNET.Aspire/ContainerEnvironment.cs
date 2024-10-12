using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageQueueNET.Aspire;

static public class ContainerEnvironment
{
    public const string DockerDesktopHost = "docker.for.mac.localhost";

    public static string HostName = Environment.MachineName;
    public static string HostAddress = DockerDesktopHost;
}
