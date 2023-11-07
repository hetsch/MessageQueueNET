using MessageQueueNET.Worker.Models.Process;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace MessageQueueNET.Worker.Services;

public partial class ProcessRunnerService
{
    private readonly ILogger<ProcessRunnerService> _logger;

    public ProcessRunnerService(ILogger<ProcessRunnerService> logger)
    {
        _logger = logger;
    }

    public Task Run(ProcessContext context)
    {
        var task = new Task(() =>
        {
            try
            {
                var processStartInfo = new ProcessStartInfo();

                processStartInfo.FileName = context.Command;
                processStartInfo.Arguments = context.Arguments;

                processStartInfo.CreateNoWindow = true;
                processStartInfo.UseShellExecute = false;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.RedirectStandardError = true;

                LogStartingProcess(context.Command, context.Arguments);

                using (Process process = Process.Start(processStartInfo)!)
                {
                    StringBuilder output = new(),
                                  stdErr = new();

                    process.OutputDataReceived += (sender, outLine) =>
                    {
                        if (!String.IsNullOrEmpty(outLine.Data))
                        {
                            //Console.WriteLine($"Output received:{ outLine.Data }");
                            output.AppendLine(outLine.Data);
                        }
                    };

                    process.ErrorDataReceived += (sender, outLine) =>
                    {
                        if (!String.IsNullOrEmpty(outLine.Data))
                        {
                            //Console.WriteLine($"Error received:{ outLine.Data }");
                            output.AppendLine(outLine.Data);
                            stdErr.AppendLine(outLine.Data);
                        }
                    };

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    process.WaitForExit();

                    // Causes Deadlogs...
                    //while (process.StandardOutput.Peek() > -1)
                    //{
                    //    output.Add(process.StandardOutput.ReadLine());
                    //}

                    //while (process.StandardError.Peek() > -1)
                    //{
                    //    output.Add(process.StandardError.ReadLine());
                    //}

                    context.ExitCode = process.ExitCode;
                    context.Output = output.ToString();
                    context.ErrorOutput = stdErr.Length > 0 ? stdErr.ToString() : null;
                }
            } 
            catch(Exception ex)
            {
                LogExceptionMessage(context.Command, ex.Message);
                throw;
            }
        });

        task.Start();

        return task;
    }

    [LoggerMessage(
        EventId = 0,
        Level = LogLevel.Information,
        Message = "Starting process {command} with arguments: {arguments}")]
    public partial void LogStartingProcess(string command, string arguments);

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Error processing command {command}: {exceptionMessage}")]
    public partial void LogExceptionMessage(string command, string exceptionMessage);
}
