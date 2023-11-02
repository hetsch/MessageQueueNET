using MessageQueueNET.Core.Models;
using MessageQueueNET.Core.Services.Abstraction;
using MessageQueueNET.Worker.Models.Process;
using MessageQueueNET.Worker.Models.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MessageQueueNET.Worker.Services.Worker;

public class CommandLineWorker : IGenericQueueProcessor<CommandLineWorkerMessage>
{
    public const string WorkerIdentifier = "mq.commandline";

    private readonly ILogger<CommandLineWorker> _logger;
    private readonly ProcessRunnerService _runnerService;

    public CommandLineWorker(ILogger<CommandLineWorker> logger,
                             ProcessRunnerService runnerService)
    {
        _logger = logger;
        _runnerService = runnerService;
    }

    public string WorkerId => WorkerIdentifier;

    public bool ConfirmAlways => true;

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

            return new GenericQueueProcessorResult<CommandLineWorkerResultBody>()
            {
                Worker = CommandLineResultFileOutputWorker.WorkerIdentifier,
                Body = new CommandLineWorkerResultBody()
                {
                    Output = proccessContext.Output,
                    ErrorOutput = proccessContext.ErrorOutput,
                    ExitCode = proccessContext.ExitCode
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError("Execute {command} {arguments} failed",
                proccessContext.Command, proccessContext.Arguments);

            return new QueueProcessorResult()
            {
                Succeeded = false,
                ErrorMessages = ex.ToString()
            };
        }
    }
}
