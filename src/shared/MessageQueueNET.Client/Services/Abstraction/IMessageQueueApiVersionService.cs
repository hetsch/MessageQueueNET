using System;

namespace MessageQueueNET.Client.Services.Abstraction;

public interface IMessageQueueApiVersionService
{
    Version Version { get; }
}
