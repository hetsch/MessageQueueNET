using MessageQueueNET.Client;
using MessageQueueNET.ProcService.Extensions;
using System;
using System.Globalization;
using System.Reflection;
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

            int maxParallelTasks = 1, queueSize = 0;
            DateTime runUntil = DateTime.MinValue;

            string logFile = String.Empty;

            if (args.Length > 0)
            {
                if (args[0] == "-version" || args[0] == "--version")
                {
                    Console.WriteLine(Version);
                    return 0;
                }

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
                            runUntil = DateTime.Now.AddSeconds(int.Parse(args[++i]));
                            break;
                        case "-stoptime":
                            DateTime td;
                            var timeString = args[++i];
                            foreach (var timeFormat in new[] { "HH:mm", "h:mm tt" })
                            {
                                if (DateTime.TryParseExact(timeString, timeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out td))
                                {
                                    if (td < DateTime.Now)
                                        td = td.AddDays(1);

                                    runUntil = td;
                                    break;
                                }
                            }
                            if (runUntil < DateTime.Now)
                            {
                                throw new Exception($"Invalid time format {timeString}. Use something like 09:00 or 14:30 or 07:00 pm");
                            }
                            break;
                        case "-log":
                            logFile = args[++i];
                            break;
                    }
                }
            }

            if (String.IsNullOrEmpty(serverUrl) ||
                String.IsNullOrEmpty(queueName) ||
                String.IsNullOrEmpty(command))
            {
                Console.WriteLine("Usage: MessageQueueNET.ProcService.exe serviceUrl -q queueName -c comand {-p max-parallel-tasks=1 -qsize queuesize=100 -duration <seconds> | -stoptime <time>  }");
                return 1;
            }

            #endregion

            try
            {
                #region Initialize Client and TaskQueue

                var cancelTracker = new CancelTracker();
                var client = new QueueClient(serverUrl, queueName);
                var taskQueue = new TaskQueue<ProccessContext>(maxParallelTasks, queueSize > 0 ? queueSize : maxParallelTasks, cancelTracker);

                taskQueue.TaskCompleted += (ProccessContext context) =>
                {
                    if (context.ExitCode > 0)
                    {
                        $"completed with exitcode {context.ExitCode} after {Math.Round((DateTime.Now - context.StartTime).TotalSeconds, 3)}s)".Log(context);
                        $"output: {context.Output}".Log(context);
                    }
                    else
                    {
                        $"completed successfully ({Math.Round((DateTime.Now - context.StartTime).TotalSeconds, 3)}s)".Log(context);
                    }
                };
                taskQueue.TaskCanceled += async (ProccessContext context) =>
                {
                    await client.EnqueueAsync(new string[] { context.Arguments });
                    $"cancelled. {context.Arguments} requeued to {queueName}".Log(context);
                };
                taskQueue.TaskCrashed += (ProccessContext context, Exception ex) =>
                {
                    $"crashed with exception:  {ex.Message}".Log(context);
                };

                #endregion

                #region Proccess Loop

                var startTime = DateTime.Now;

                if (runUntil > DateTime.Now)
                {
                    $"Application running until {runUntil.ToShortDateString()} {runUntil.ToLongTimeString()}".Log(logFile: logFile);
                }
                $"Service triggers {command}".Log(logFile: logFile);
                $"Service monitors queue {queueName}".Log(logFile: logFile);
                "Press Ctrl-C to stop...".Log(logFile: logFile);

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
                        var messagesResult = await client.DequeueAsync(taskQueue.CurrentCapacity);
                        if (messagesResult?.Messages != null)
                        {
                            foreach (var message in messagesResult.Messages)
                            {
                                var task = taskQueue.AwaitRequest(new ProcessTask().Run, new ProccessContext()
                                {
                                    Command = command,
                                    Arguments = message.Value ?? string.Empty,
                                    LogFile = logFile
                                });
                            }
                        }
                    }

                    await Task.Delay(1000);

                    if (!runUntil.Equals(DateTime.MinValue) && runUntil <= DateTime.Now)
                    {
                        cancelTracker.Cancel();
                    }
                }

                #endregion

                #region Cleanup

                "Finishing tasks...".Log(logFile: logFile); ;

                var runningTasks = 0;
                while (taskQueue.HasRunningTasks)
                {
                    if (taskQueue.RunningTask != runningTasks)
                    {
                        runningTasks = taskQueue.RunningTask;
                        $"Waiting for {runningTasks} tasks to finsish...".Log(logFile: logFile); ;
                    }
                    await Task.Delay(1000);
                }

                #endregion

                return 0;
            }
            catch (Exception ex)
            {
                $"Ends with exception: {ex.Message}".Log(logFile: logFile); ;
                return 1;
            }
        }

        public static string Version =>
            Assembly
            .GetAssembly(typeof(MessageQueueNET.Client.QueueClient))!
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
            .InformationalVersion;
    }
}
