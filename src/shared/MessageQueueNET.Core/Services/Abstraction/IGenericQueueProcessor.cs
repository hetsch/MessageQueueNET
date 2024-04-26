using MessageQueueNET.Core.Models;
using System.Threading;
using System.Threading.Tasks;

namespace MessageQueueNET.Core.Services.Abstraction;
public interface IGenericQueueProcessor<T> : IQueueProcessor
    where T : new()
{
    Task<QueueProcessorResult> ProcessGeneric(GenericQueueProcessorMessage<T> message, CancellationToken cancellationToken);
}
