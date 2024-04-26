using System;

namespace MessageQueueNET.Core.Reflection;

[AttributeUsage(AttributeTargets.Class)]
public class MessageHandlerAttribute : Attribute
{
    public MessageHandlerAttribute() { }

    public string CommandName { get; set; } = "";
}
