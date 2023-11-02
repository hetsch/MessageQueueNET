using MessageQueueNET.Core.Models;
using MessageQueueNET.Core.Services.Abstraction;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MessageQueueNET.Client.Services;
internal class PingWorker : INonGenericQueueProcessor
{
    public const string WorkerIdentifier = "mq.ping";

    public string WorkerId => WorkerIdentifier;

    public bool ConfirmAlways => true;

    public Task<QueueProcessorResult> Process(BaseQueueProcessorMessage jobProcessMessage, CancellationToken cancellationToken)
    {
        if (jobProcessMessage is not null)
        {
            jobProcessMessage.ResultQueue += ".ping";
        }

        return Task.FromResult(new QueueProcessorResult());
    }
}
