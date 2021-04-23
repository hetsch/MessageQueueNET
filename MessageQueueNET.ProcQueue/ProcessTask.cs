using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageQueueNET.ProcQueue
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

                processStartInfo.UseShellExecute = true;
                processStartInfo.RedirectStandardOutput = false;

                Console.WriteLine($"Starting process { context.ProcId }: { context.Command } { context.Arguments }");

                using (Process process = Process.Start(processStartInfo))
                {
                    process.WaitForExit();

                    context.ExitCode = process.ExitCode;
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
