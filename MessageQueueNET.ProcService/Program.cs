using MessageQueueNET.Client;
using MessageQueueNET.ProcService.Extensions;
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
                        $"completed with exitcode { context.ExitCode } after { Math.Round((DateTime.Now - context.StartTime).TotalSeconds, 3) }s)".Log(context);
                        $"output: { context.Output }".Log(context);
                    }
                    else
                    {
                        $"completed successfully ({ Math.Round((DateTime.Now - context.StartTime).TotalSeconds, 3) }s)".Log(context);
                    }
                };
                taskQueue.TaskCanceled += async (ProccessContext context) =>
                {
                    await client.Enqueue(new string[] { context.Arguments });
                    $"cancelled. { context.Arguments } requeued to { queueName }".Log(context);
                };
                taskQueue.TaskCrashed += (ProccessContext context, Exception ex) =>
                {
                    $"crashed with exception:  { ex.Message }".Log(context);
                };

                #endregion

                #region Proccess Loop

                var startTime = DateTime.Now;

                if (runForSeconds > 0)
                {
                    Console.WriteLine($"Application running until { startTime.AddSeconds(runForSeconds).ToShortDateString() } { startTime.AddSeconds(runForSeconds).ToLongTimeString() }");
                }
                Console.WriteLine($"Service triggers { command }");
                Console.WriteLine($"Service monitors queue { queueName }");
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

                var runningTasks = 0;
                while (taskQueue.HasRunningTasks)
                {
                    if (taskQueue.RunningTask != runningTasks)
                    {
                        runningTasks = taskQueue.RunningTask;
                        Console.WriteLine($"Waiting for { runningTasks } tasks to finsish...");
                    }
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
