using System;

namespace MessageQueueNET.Core.Concurrency;

public class MessageQueueFuzzyMutexAsyncExcepiton : Exception
{
    public MessageQueueFuzzyMutexAsyncExcepiton(string message)
            : base(message)
    { }
}
