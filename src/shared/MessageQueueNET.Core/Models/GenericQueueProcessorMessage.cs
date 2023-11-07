namespace MessageQueueNET.Core.Models;

public class GenericQueueProcessorMessage<T> : BaseQueueProcessorMessage
    where T : new()
{
    public T? Body { get; set; }
}
