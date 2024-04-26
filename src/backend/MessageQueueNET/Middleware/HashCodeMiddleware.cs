using MessageQueueNET.Extensions;
using MessageQueueNET.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MessageQueueNET.Middleware;

public class HashCodeMiddleware
{
    private readonly RequestDelegate _next;

    public HashCodeMiddleware(RequestDelegate next)
    {
        _next = next;
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
                while ((DateTime.Now - start).TotalSeconds < 60)
                {
                    var queues = queueService.GetQueues(idPattern!, false);

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
