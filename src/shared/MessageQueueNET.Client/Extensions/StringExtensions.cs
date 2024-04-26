using MessageQueueNET.Core.Models;
using System;
using System.Text.Json;

namespace MessageQueueNET.Client.Extensions;
static internal class StringExtensions
{
    static public object? DeserializeJobProcessingMessage(this string jsonString, Type bodyType)
    {
        var genericType = typeof(GenericQueueProcessorMessage<>).MakeGenericType(bodyType);

        return JsonSerializer.Deserialize(jsonString, genericType);
    }
}
