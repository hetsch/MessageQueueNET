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

        public string ServerUrl { get; set; } = string.Empty;
        public string QueueName { get; set; } = string.Empty;
        public string Command { get; set; } = string.Empty;
        public int? LifetimeSeconds { get; set; }
        public int? ItemLifetimeSeconds { get; set; }
        public int? ConfirmationPeridSeconds { get; set; }
        public int? MaxUnconfirmedItems { get; set; }
        public bool? SuspendEnqueue { get; set; }
        public bool? SuspendDequeue { get; set; }
        public string WorkerCommand { get; set; }
        public bool PingWorker { get; set; } = false;
        public List<string> Messages { get; set; }
        public Guid MessageId { get; set; }
        public bool ShowFullItem { get; set; }

        public bool UnconfirmedOnly { get; set; }
        public int Max { get; set; }

        public void Parse(string[] args, int startAt = 0)
        {
            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-q":
                        this.QueueName = args[++i];
                        break;
                    case "-c":
                        this.Command = args[++i];
                        break;
                    case "-workercmd":
                        this.WorkerCommand = args[++i];
                        break;
                    case "-ping":
                        this.PingWorker = true;
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
                    case "-confirmationperiod":
                        this.ConfirmationPeridSeconds = int.Parse(args[++i]);
                        break;
                    case "-maxunconfirmeditems":
                        this.MaxUnconfirmedItems = int.Parse(args[++i]);
                        break;
                    case "-id":
                    case "-messageid":
                        this.MessageId = new Guid(args[++i]);
                        break;
                    case "-full":
                        ShowFullItem = true;
                        break;
                    case "-max":
                        this.Max = int.Parse(args[++i]);
                        break;
                    case "-unconfirmed":
                        this.UnconfirmedOnly = true;
                        break;
                }
            }
        }
    }
}
