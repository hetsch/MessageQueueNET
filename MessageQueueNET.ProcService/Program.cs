using MessageQueueNET.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MessageQueueNET.ProcService
{
    class Program
    {
        async static Task<int> Main(string[] args)
        {
            #region Parse Command Line

            string serverUrl = String.Empty, 
                   queueName = String.Empty,
                   command = String.Empty;

            int maxParallelTasks = 1, queueSize = 100;
            int runForSeconds = 0;

            if (args.Length > 0)
            {
                serverUrl = args[0];

                for (int i = 1; i < args.Length - 1; i++)
                {
                    switch (args[i])
                    {
                        case "-q":
                            queueName = args[++i];
                            break;
                        case "-c":
                            command = args[++i];
                            break;
                        case "-p":
                            maxParallelTasks = int.Parse(args[++i]);
                            break;
                        case "-qsize":
                            queueSize = int.Parse(args[++i]);
                            break;
                        case "-duration":
                            runForSeconds = int.Parse(args[++i]);
                            break;
                    }
                }
            }

            if (String.IsNullOrEmpty(serverUrl) ||
                String.IsNullOrEmpty(queueName) ||
                String.IsNullOrEmpty(command))
            {
                Console.WriteLine("Usage: MessageQueueNET.ProcService.exe serviceUrl -q queueName -c comand {-p max-parallel-tasks=1 -qsize queuesize=100}");
                Console.WriteLine("       command: remove, enqueue");
                return 1;
            }

            #endregion

            try
            {
                #region Initialize Client and TaskQueue

                var cancelTracker = new CancelTracker();
                var client = new QueueClient(serverUrl, queueName);
                var taskQueue = new TaskQueue<ProccessContext>(maxParallelTasks, queueSize, cancelTracker);

                taskQueue.TaskCompleted += (ProccessContext context) =>
                {
                    if (context.ExitCode > 0)
                    {
                        Console.WriteLine($"Task { context.ProcId } completed with exitcode { context.ExitCode }{ Environment.NewLine }{ context.Output }");
                    }
                    else
                    {
                        Console.WriteLine($"Task { context.ProcId } completed successfully");
                    }
                };
                taskQueue.TaskCanceled += async (ProccessContext context) =>
                {
                    await client.Enqueue(new string[] { context.Arguments });
                };
                taskQueue.TaskCrashed += (ProccessContext context, Exception ex) =>
                {
                    Console.WriteLine($"Task { context.ProcId } crashed with exception:  { ex.Message }");
                };

                #endregion

                #region Proccess Loop

                var startTime = DateTime.Now;

                if (runForSeconds > 0)
                {
                    Console.WriteLine($"Application running until { startTime.AddSeconds(runForSeconds).ToShortDateString() } { startTime.AddSeconds(runForSeconds).ToLongTimeString() }");
                }
                Console.WriteLine("Press Ctrl-C to stop...");

                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    cancelTracker.Cancel();
                };

                while (!cancelTracker.IsCancelled)
                {
                    var currentCapacity = taskQueue.CurrentCapacity;
                    if (currentCapacity > 0)
                    {
                        foreach (var message in await client.Dequeue(taskQueue.CurrentCapacity))
                        {
                            var task = taskQueue.AwaitRequest(new ProcessTask().Run, new ProccessContext()
                            {
                                Command = command,
                                Arguments = message
                            });
                        }
                    }

                    await Task.Delay(1000);

                    if (runForSeconds > 0 && startTime.AddSeconds(runForSeconds) <= DateTime.Now)
                    {
                        cancelTracker.Cancel();
                    }
                }

                #endregion

                #region Cleanup

                Console.WriteLine("Finishing tasks...");

                while(taskQueue.HasRunningTasks)
                {
                    Console.WriteLine($"Waiting for { taskQueue.RunningTask } tasks to finsish...");
                    await Task.Delay(1000);
                }

                #endregion

                return 0;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Ends with exception: { ex.Message }");
                return 1;
            }
        }
    }
}
