using MessageQueueNET.Core.Models;
using MessageQueueNET.Core.Services.Abstraction;
using MessageQueueNET.Worker.Models.Worker;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MessageQueueNET.Worker.Services.Worker;

internal class CommandLineResultConsoleOutputWorker : IGenericQueueProcessor<CommandLineWorkerResultBody>
{
    public const string WorkerIdentifier = "mq-commandline.result.console";

    public CommandLineResultConsoleOutputWorker()
    {
    }

    public string WorkerId => WorkerIdentifier;

    public bool ConfirmAlways => true;

    public Task<QueueProcessorResult> ProcessGeneric(GenericQueueProcessorMessage<CommandLineWorkerResultBody> message, CancellationToken cancellationToken)
    {
        StringBuilder output = new StringBuilder();

        output.AppendLine($"ProcessId: {message.ProcessId}");
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

        System.Console.WriteLine(output.ToString());

        return Task.FromResult(new QueueProcessorResult());
    }
}
