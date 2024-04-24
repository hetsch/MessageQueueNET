using MessageQueueNET.Extensions;
using MessageQueueNET.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MessageQueueNET.Middleware;

public class HashCodeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly int _maxPollingSeonds = 60;

    public HashCodeMiddleware(
                RequestDelegate next, 
                IConfiguration configuration,
                ILogger<HashCodeMiddleware> logger
            )
    {
        _next = next;

        var maxPollingSeconds = configuration["MessageQueue:MaxRequestPollingSeconds"];
        if(!String.IsNullOrEmpty(maxPollingSeconds))
        {
            _maxPollingSeonds = int.Parse(maxPollingSeconds);
        }

        logger.LogInformation("Max request polling duration set to {maxRequestPolling} seconds", _maxPollingSeonds);
    }

    public async Task InvokeAsync(HttpContext context,
                                  QueuesService queueService)
    {
        string? idPattern = context.Request.Path.Value?.Split('/').Last();

        if (context.Request.TryGetHashCode(out var hashCode)
            && !String.IsNullOrEmpty(idPattern))
        {
            if (!String.IsNullOrEmpty(hashCode))
            {
                DateTime start = DateTime.Now;
                int maxPollingSeconds = context.Request.MaxPollingSeconds(_maxPollingSeonds);
                bool forAccess = !context.Request.IsSlientAccess();

                while ((DateTime.Now - start).TotalSeconds < _maxPollingSeonds)
                {
                    var queues = queueService.GetQueues(idPattern!, forAccess);

                    if (queues.CalcHashCode().ToString() != hashCode)
                    {
                        break;
                    }

                    await Task.Delay(100);
                }
            }
        }

        await _next(context);
    }
}
