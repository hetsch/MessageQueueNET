using MessageQueueNET.Core.Concurrency;
using MessageQueueNET.Core.Models;
using MessageQueueNET.Core.Services.Abstraction;
using MessageQueueNET.Worker.Models.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MessageQueueNET.Worker.Services.Worker;

internal class CommandLineResultFileOutputWorker : IGenericQueueProcessor<CommandLineWorkerResultBody>
{
    public const string WorkerIdentifier = "mq-commandline.result.file";

    private readonly ILogger<CommandLineResultFileOutputWorker> _logger;
    private readonly CommandLineResultFileOutputWorkerOptions _options;

    public CommandLineResultFileOutputWorker(ILogger<CommandLineResultFileOutputWorker> logger,
                                             IOptions<CommandLineResultFileOutputWorkerOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public string WorkerId => WorkerIdentifier;

    public bool ConfirmAlways => true;

    async public Task<QueueProcessorResult> ProcessGeneric(GenericQueueProcessorMessage<CommandLineWorkerResultBody> message,
                                                           CancellationToken cancellationToken)
    {
        try
        {
            if (!String.IsNullOrEmpty(_options.RootPath))
            {
                StringBuilder output = new StringBuilder();

                output.AppendLine();
                output.AppendLine($"{DateTime.Now}: ProcessId: {message.ProcessId}");
                if (message.Body != null)
                {
                    output.AppendLine($"ExitCode: {message.Body.ExitCode}");
                    if (!String.IsNullOrEmpty(message.Body.ErrorOutput))
                    {
                        output.AppendLine($"Error: {message.Body.ErrorOutput}");
                    }
                    else
                    {
                        output.AppendLine(message.Body.Output);
                    }
                }

                string filename = Path.Combine(_options.RootPath, $"{message.ProcessId}.log");
                using (var mutext = await MessageQueueFuzzyMutexAsync.LockAsync(filename))
                {
                    if (!Directory.Exists(_options.RootPath))
                    {
                        Directory.CreateDirectory(_options.RootPath);
                    }

                    _logger.LogInformation("Log console process output to {consoleOutputFilename}", filename);
                    await File.AppendAllTextAsync(filename, output.ToString(), cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Commandline Procces logging failed: {message}", ex.Message);
        }
        return new QueueProcessorResult();
    }
}
