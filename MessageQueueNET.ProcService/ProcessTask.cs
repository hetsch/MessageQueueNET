using MessageQueueNET.ProcService.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

                $"Starting process { context.ProcId } with: { context.Arguments }".Log();

                using (Process process = Process.Start(processStartInfo))
                {
                    process.WaitForExit();

                    var output = new List<string>();

                    while (process.StandardOutput.Peek() > -1)
                    {
                        output.Add(process.StandardOutput.ReadLine());
                    }

                    while (process.StandardError.Peek() > -1)
                    {
                        output.Add(process.StandardError.ReadLine());
                    }

                    

                    context.ExitCode = process.ExitCode;
                    context.Output = String.Join('\n', output);

                    //context.Output = process.StandardOutput.ReadToEnd();
                    

                    //if (context.ExitCode > 0)
                    //{
                    //    Console.WriteLine($"Exit Code: { context.ExitCode }");
                    //    Console.WriteLine($"Error:\n{ context.EOutput }");
                    //}

                    //Console.WriteLine($"Finished process { context.ProcId }");
                }
            });

            task.Start();

            return task;
        }
    }
}
