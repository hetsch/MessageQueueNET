using System;

namespace MessageQueueNET.ProcService.Abstraction
{
    public interface ITaskContext
    {
        long TaskId { get; set; }
        DateTime StartTime { get; set; }

        string LogFile { get; set; }
    }
}
