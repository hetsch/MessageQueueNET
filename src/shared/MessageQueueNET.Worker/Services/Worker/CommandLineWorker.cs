using MessageQueueNET.Core.Models;
using MessageQueueNET.Core.Services.Abstraction;
using MessageQueueNET.Worker.Extensions;
using MessageQueueNET.Worker.Models.Process;
using MessageQueueNET.Worker.Models.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MessageQueueNET.Worker.Services.Worker;

public class CommandLineWorker : IGenericQueueProcessor<CommandLineWorkerMessage>
{
    public const string WorkerIdentifier = "mq.commandline";

    private readonly ILogger<CommandLineWorker> _logger;
    private readonly ProcessRunnerService _runnerService;
    private readonly CommandLineWorkerOptions _options;

    public CommandLineWorker(ILogger<CommandLineWorker> logger,
                             ProcessRunnerService runnerService,
                             IOptions<CommandLineWorkerOptions> options)
    {
        _logger = logger;
        _runnerService = runnerService;
        _options = options.Value;
    }

    public string WorkerId => WorkerIdentifier;

    public bool ConfirmAlways => true;

    async public Task<QueueProcessorResult> ProcessGeneric(GenericQueueProcessorMessage<CommandLineWorkerMessage> message, CancellationToken cancellationToken)
    {
        ProcessContext? proccessContext = null;

        try
        {
            if (message?.Body is null)
            {
                throw new Exception("No message body received");
            }

            if (!message.Body.Command.FitsAnyPattern(_options.CommandFilters))
            {
                throw new Exception($"Command '{message.Body.Command}' not allowed (do not fits any pattern)");
            }

            proccessContext = new ProcessContext()
            {
                Command = message.Body.Command,
                Arguments = message.Body.Arguments
            };


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
            if (proccessContext is not null)
            {
                _logger.LogError("Execute {command} {arguments} failed. {message}",
                    proccessContext.Command, proccessContext.Arguments, ex.Message);
            }
            else
            {
                _logger.LogError("Excute command failed. {message}", ex.Message);
            }

            return new QueueProcessorResult()
            {
                Succeeded = false,
                ErrorMessages = ex.ToString()
            };
        }
    }
}
