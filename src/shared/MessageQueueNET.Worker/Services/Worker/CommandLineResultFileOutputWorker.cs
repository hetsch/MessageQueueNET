using MessageQueueNET.Core.Models;
using MessageQueueNET.Core.Services.Abstraction;
using MessageQueueNET.Worker.Models.Worker;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MessageQueueNET.Worker.Services.Worker;

internal class CommandLineResultFileOutputWorker : IGenericQueueProcessor<CommandLineWorkerResultBody>
{
    public const string WorkerIdentifier = "mq-commandline.result.file";

    public CommandLineResultFileOutputWorker()
    {
    }

    public string WorkerId => WorkerIdentifier;

    public bool ConfirmAlways => true;

    async public Task<QueueProcessorResult> ProcessGeneric(GenericQueueProcessorMessage<CommandLineWorkerResultBody> message, 
                                                           CancellationToken cancellationToken)
    {
        StringBuilder output = new StringBuilder();

        output.AppendLine();
        output.AppendLine($"{DateTime.Now.ToString()}: ProcessId: {message.ProcessId}");
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

        await File.AppendAllTextAsync($"c:\\temp\\{message.ProcessId}.log", output.ToString(), cancellationToken);

        return new QueueProcessorResult();
    }
}
