using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MessageQueueNET.Cmd
{
    class CmdArguments
    {
        public CmdArguments()
        {
            this.Messages = new List<string>();
        }

        public string ServerUrl { get; set; }
        public string QueueName { get; set; }
        public string Command { get; set; }
        public int? LifetimeSeconds { get; set; }
        public int? ItemLifetimeSeconds { get; set; }
        public int? ConfirmProcessingSeconds { get; set; }
        public bool? SuspendEnqueue { get; set; }
        public bool? SuspendDequeue { get; set; }
        public List<string> Messages { get; set; }
        public bool ShowFullItem { get; set; }

        public void Parse(string[] args, int startAt = 0)
        {
            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-q":
                        this.QueueName = args[++i];
                        break;
                    case "-c":
                        this.Command = args[++i];
                        break;
                    case "-m":
                        this.Messages.Add(args[++i]);
                        break;
                    case "-f":
                        this.Messages.AddRange(File.ReadAllLines(args[++i])
                                .Select(l => l.Trim())
                                .Where(l => !String.IsNullOrEmpty(l)));
                        break;
                    case "-lifetime":
                        this.LifetimeSeconds = int.Parse(args[++i]);
                        break;
                    case "-itemlifetime":
                        this.ItemLifetimeSeconds = int.Parse(args[++i]);
                        break;
                    case "-suspend-enqueue":
                        this.SuspendEnqueue = bool.Parse(args[++i]);
                        break;
                    case "-suspend-dequeue":
                        this.SuspendDequeue = bool.Parse(args[++i]);
                        break;
                    case "-confirmProcessingSeconds":
                        this.ConfirmProcessingSeconds = int.Parse(args[++i]);
                        break;
                    case "-full":
                        ShowFullItem = true;
                        break;
                }
            }
        }
    }
}
