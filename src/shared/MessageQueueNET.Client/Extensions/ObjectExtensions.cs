using MessageQueueNET.Core.Models;
using MessageQueueNET.Core.Services.Abstraction;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MessageQueueNET.Client.Extensions;
internal static class ObjectExtensions
{
    public static bool TryGetGenericJobProcessorType(this object obj, [NotNullWhen(true)] out Type? genericType)
    {
        genericType = null;

        if (obj == null)
        {
            return false;
        }

        var objectType = obj.GetType();
        var interfaceType = objectType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IGenericQueueProcessor<>));

        if (interfaceType != null)
        {
            genericType = interfaceType.GetGenericArguments().First();

            return true;
        }
        return false;
    }

    public static Task<QueueProcessorResult> CallProcessGeneric(this IQueueProcessor processor, object message, CancellationToken cancellationToken)
    {
        var task = processor.GetType()
                      .GetMethod("ProcessGeneric")!
                      .Invoke(processor, new object[] { message, cancellationToken }) as Task<QueueProcessorResult>;

        if (task is null)
        {
            throw new Exception("Task is not a valid Task<JobProcessorResult> Type");
        }

        return task;
    }
}
