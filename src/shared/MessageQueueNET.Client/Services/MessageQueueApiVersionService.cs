using MessageQueueNET.Client.Services.Abstraction;
using System;
using System.Reflection;

namespace MessageQueueNET.Client.Services
{
    public class MessageQueueApiVersionService : IMessageQueueApiVersionService
    {
        private readonly Version _version;

        public MessageQueueApiVersionService()
        {
            _version = Assembly
                .GetAssembly(typeof(QueueClient))!
                .GetName()!
                .Version!;
        }

        public Version Version => _version;
    }
}
