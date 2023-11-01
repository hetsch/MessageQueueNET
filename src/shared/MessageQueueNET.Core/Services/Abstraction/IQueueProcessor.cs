using MessageQueueNET.Core.Models;

namespace MessageQueueNET.Core.Services.Abstraction;

public interface IQueueProcessor
{
    bool CanProcessMessage(BaseQueueProcessorMessage jobProcessMessage);
}
