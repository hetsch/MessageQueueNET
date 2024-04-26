using MessageQueueNET.Client;
using Microsoft.AspNetCore.Http;

namespace MessageQueueNET.Extensions;

static internal class HttpRequestExtensions
{
    static public string GetClientId(this HttpRequest httpRequest)
    {
        httpRequest.Headers.TryGetValue(MQHeaders.ClientId, out var hashCode);

        return hashCode.ToString();
    }

    static public string GetHashCode(this HttpRequest httpRequest)
    {
        httpRequest.TryGetHashCode(out var hashCode);

        return hashCode;
    }

    static public bool TryGetHashCode(this HttpRequest httpRequest, out string hashCode)
    {
        if (!httpRequest.Headers.TryGetValue(MQHeaders.HashCode, out var result))
        {
            hashCode = "";

            return false;
        }

        hashCode = result.ToString();

        return true;
    }
}
