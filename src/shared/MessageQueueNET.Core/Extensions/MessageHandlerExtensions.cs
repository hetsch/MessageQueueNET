using MessageQueueNET.Core.Reflection;
using MessageQueueNET.Core.Services.Abstraction;
using System;
using System.Reflection;

namespace MessageQueueNET.Core.Extensions;
static public class MessageHandlerExtensions
{
    static public string CommandName(this IMessageHandler messageHandler)
        => MessageHandlerCommandName(messageHandler?.GetType());

    static public string MessageHandlerCommandName(this Type? messageHandlerType)
    {
        var handlerAttribute = messageHandlerType?.GetCustomAttribute<MessageHandlerAttribute>();

        return String.IsNullOrEmpty(handlerAttribute?.CommandName) switch
        {
            true => messageHandlerType?.Name ?? "",
            false => handlerAttribute!.CommandName
        };
    }
}
