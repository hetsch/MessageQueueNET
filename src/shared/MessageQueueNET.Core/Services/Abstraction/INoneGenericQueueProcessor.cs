using MessageQueueNET.Core.Models;
using System.Threading;
using System.Threading.Tasks;

namespace MessageQueueNET.Core.Services.Abstraction;

public interface INoneGenericQueueProcessor : IQueueProcessor
{
    Task<QueueProcessorResult> Process(BaseQueueProcessorMessage jobProcessMessage, CancellationToken cancellationToken);
}