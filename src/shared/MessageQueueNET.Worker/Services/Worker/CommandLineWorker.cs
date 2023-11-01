using MessageQueueNET.Core.Models;
using MessageQueueNET.Core.Services.Abstraction;
using MessageQueueNET.Worker.Models.Process;
using MessageQueueNET.Worker.Models.Worker;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MessageQueueNET.Worker.Services.Worker;

internal class CommandLineWorker : IGenericQueueProcessor<CommandLineWorkerMessage>
{
    private readonly ProcessRunnerService _runnerService;

    public CommandLineWorker(ProcessRunnerService runnerService)
    {
        _runnerService = runnerService;
    }

    public bool CanProcessMessage(BaseQueueProcessorMessage jobProcessMessage)
    {
        return "commandline".Equals(jobProcessMessage.JobType, StringComparison.OrdinalIgnoreCase);
    }

    async public Task<QueueProcessorResult> ProcessGeneric(GenericQueueProcessorMessage<CommandLineWorkerMessage> message, CancellationToken cancellationToken)
    {
        if (message?.Body is null)
        {
            throw new Exception("No message body received");
        }

        var proccessContext = new ProcessContext()
        {
            Command = message.Body.Command,
            Arguments = message.Body.Arguments
        };

        try
        {

            await _runnerService.Run(proccessContext);

            return new QueueProcessorResult()
            {
                Confirm = true,
                Body = new
                {
                    Output = proccessContext.Output,
                    ExitCode = proccessContext.ExitCode
                }
            };
        } 
        catch (Exception ex)
        {
            return new QueueProcessorResult()
            {
                Succeeded = false,
                Confirm = true,
                ErrorMessages = ex.ToString()
            };
        }
    }
}
