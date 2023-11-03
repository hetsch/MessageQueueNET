using MessageQueueNET.Client.Models;
using System;

namespace MessageQueueNET.Extensions;

internal static class ApiResultExtensions
{
    static public T AddExceptionMessage<T>(this T apiResult, Exception ex)
        where T : ApiResult
    {
        apiResult.Success = false;
        apiResult.ErrorMessage = ex.Message;

        return apiResult;
    }
}
