using MessageQueueNET.Services.Abstraction;
using System.Reflection;

namespace MessageQueueNET.Services
{
    public class AppVersionService : IAppVersionService
    {
        //public string Version =>
        //    Assembly
        //    .GetAssembly(typeof(MessageQueueNET.Client.QueueClient))!
        //    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
        //    .InformationalVersion;

        public string Version =>
            Assembly
            .GetAssembly(typeof(MessageQueueNET.Client.QueueClient))!
            .GetName()!
            .Version!.ToString();
    }
}
