namespace MessageQueueNET.Core.Models;

public class GenericQueueProcessorResult<T> : QueueProcessorResult
{
    new public T? Body { get; set; }
}