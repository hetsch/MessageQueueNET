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

    static public (string key, string val) SplitByFirst(this string? str, char separator)
    {
        if (string.IsNullOrEmpty(str) || !str.Contains(separator))
        {
            return (str ?? string.Empty, string.Empty);
        }

        string key = str.Substring(0, str.IndexOf(separator));
        string val = str.Substring(key.Length + 1);

        return (key, val);
    }
}
