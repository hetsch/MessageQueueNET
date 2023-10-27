using MessageQueueNET.Client.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace MessageQueueNET.Client.Extensions;

static internal class Result
{
    static public T Deserialize<T>(string json, JsonSerializerOptions jsonOptions)
        where T : ApiResult
    {
        var apiResult = JsonSerializer.Deserialize<T>(json, jsonOptions);

        if (apiResult is null)
        {
            throw new InvalidOperationException("Deserialization returned null.");
        }

        if (apiResult.Success == false)
        {
            throw new Exception(apiResult.ErrorMessage ?? "Unkown error");
        }

        return apiResult;
    }
}
