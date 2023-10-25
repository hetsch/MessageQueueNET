using MessageQueueNET.ProcService.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MessageQueueNET.ProcService
{
    class ProcessTask
    {
        public ProcessTask()
        {
        }

        public Task Run(ProccessContext context)
        {
            var task = new Task(() =>
            {
                var processStartInfo = new ProcessStartInfo();

                processStartInfo.FileName = context.Command;
                processStartInfo.Arguments = context.Arguments;

                processStartInfo.CreateNoWindow = true;
                processStartInfo.UseShellExecute = false;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.RedirectStandardError = true;

                $"Starting process - arguments: {context.Arguments}".Log(context);

                using (Process process = Process.Start(processStartInfo)!)
                {
                    var output = new List<string>();

                    process.OutputDataReceived += (sender, outLine) =>
                    {
                        if (!String.IsNullOrEmpty(outLine.Data))
                        {
                            //Console.WriteLine($"Output received:{ outLine.Data }");
                            output.Add(outLine.Data);
                        }
                    };

                    process.ErrorDataReceived += (sender, outLine) =>
                    {
                        if (!String.IsNullOrEmpty(outLine.Data))
                        {
                            //Console.WriteLine($"Error received:{ outLine.Data }");
                            output.Add(outLine.Data);
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
                    context.Output = String.Join('\n', output);
                }
            });

            task.Start();

            return task;
        }
    }
}
