using MessageQueueNET.Core.Models;
using MessageQueueNET.Core.Services.Abstraction;
using System;
using System.Threading.Tasks;

namespace MessageQueueNET.Client.Services;
internal class PingWorker : IBaseQueueProcessor
{
    public bool CanProcessMessage(BaseQueueProcessorMessage jobProcessMessage)
        => "ping".Equals(jobProcessMessage.JobType, StringComparison.OrdinalIgnoreCase);

    public Task<QueueProcessorResult> Process(BaseQueueProcessorMessage jobProcessMessage)
    {
        if (jobProcessMessage is not null)
        {
            jobProcessMessage.ResultQueue += ".ping";
        }

        return Task.FromResult(new QueueProcessorResult());
    }
}
