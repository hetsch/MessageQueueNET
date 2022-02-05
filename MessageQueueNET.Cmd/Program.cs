using MessageQueueNET.Client;
using MessageQueueNET.Client.Models;
using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MessageQueueNET.Cmd
{
    class Program
    {
        async static Task<int> Main(string[] args)
        {
            var cmdArguments = new CmdArguments();

            if (args.Length > 0)
            {
                if (args[0] == "-version" || args[0] == "--version")
                {
                    Console.WriteLine(Version);
                    return 0;
                }

                cmdArguments.ServerUrl = args[0];
                cmdArguments.Parse(args, 1);
            }
            if (String.IsNullOrEmpty(cmdArguments.ServerUrl) ||
                String.IsNullOrEmpty(cmdArguments.Command))
            {
                WriteHelp();
                return 1;
            }

            try
            {
                if (cmdArguments.Command == "shell")
                {
                    await Shell(cmdArguments.ServerUrl);
                    return 0;
                }
                else if (String.IsNullOrEmpty(cmdArguments.QueueName))
                {
                    WriteHelp();
                    return 1;
                }

                await Exec(cmdArguments);

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: { ex.Message }");
                return 1;
            }
        }

        static private void WriteHelp()
        {
            Console.WriteLine("Usage: MessageQueueNET.Cmd.exe serviceUrl -q queueName -c comand {-m message | -f messages-file}");
            Console.WriteLine("       command: remove, enqueue, dequeue, length, queuenames, register, properties, all, shell");
        }

        static private void WriteShellHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine();

            Console.WriteLine("queuenames => list all queue names");
            Console.WriteLine();

            Console.WriteLine("<queuename> <command> [options] => run a command on a queue");
            Console.WriteLine();

            Console.WriteLine("Commands:");

            Console.WriteLine("  register: register a new q1 or change the properties of an existing queue");
            Console.WriteLine("  ----------------------------------------------------------------------------------------------------");
            Console.WriteLine("  options:");
            Console.WriteLine("     -lifetime <number>          : The lifetime of the queue seconds if queue get empty. 0 => queue will never removed automatically");
            Console.WriteLine("     -itemlifetime <number>      : The lifetime of items in seconds. 0 => items will never removed automatically");
            Console.WriteLine("     -confirmationperiod <number>: Seconds a client can wail until confirming the messeage.");
            Console.WriteLine("                                   if the message will not confirmed by the client, it will re-enqued");
            Console.WriteLine("                                   0 => no confirmation needed");
            Console.WriteLine("     -suspend-enqueue <true|false>: suspend enqueue items");
            Console.WriteLine("     -suspend-dequeue <true|false>: suspend dequeue items");
            Console.WriteLine();

            Console.WriteLine("  properties: Lists the properties of an queue");
            Console.WriteLine("  ----------------------------------------------------------------------------------------------------");
            Console.WriteLine();

            Console.WriteLine("  remove: removes/destroys a queue");
            Console.WriteLine("  ----------------------------------------------------------------------------------------------------");
            Console.WriteLine();

            Console.WriteLine("  length: number of messages in a queue");
            Console.WriteLine("  ----------------------------------------------------------------------------------------------------");
            Console.WriteLine();

            Console.WriteLine("  all: lists all messages in a queue without dequeing any item");
            Console.WriteLine("  ----------------------------------------------------------------------------------------------------");
            Console.WriteLine("  options: (all optional)");
            Console.WriteLine("     -max <max number of results>");
            Console.WriteLine("     -unconfirmed   ... show only the unconfirmed messages");
            Console.WriteLine();

            Console.WriteLine("  enqueue: enqueies messages");
            Console.WriteLine("  ----------------------------------------------------------------------------------------------------");
            Console.WriteLine("  options:");
            Console.WriteLine("     -m message1 -m message2");
            Console.WriteLine();

            Console.WriteLine("  dequeue: dequeues a message");
            Console.WriteLine("  ----------------------------------------------------------------------------------------------------");
            Console.WriteLine();

            Console.WriteLine("  confirm,confirmdequeue: dequeues a message");
            Console.WriteLine("  ----------------------------------------------------------------------------------------------------");
            Console.WriteLine("  options:");
            Console.WriteLine("     -id or -messageid MessageId (guid)");

            Console.WriteLine();
        }

        async static private Task Exec(CmdArguments cmdArguments)
        {
            var client = new QueueClient(cmdArguments.ServerUrl, cmdArguments.QueueName);

            if (cmdArguments.Command == "remove")
            {
                if (!await client.RemoveAsync())
                {
                    throw new Exception("Can't remove queue");
                }
            }
            else if (cmdArguments.Command == "enqueue")
            {
                if (!await client.EnqueueAsync(cmdArguments.Messages))
                {
                    throw new Exception($"Can't enqueue messages...");
                }
            }
            else if (cmdArguments.Command == "dequeue")
            {
                var messagesResult = await client.DequeueAsync();
                if (messagesResult?.Messages != null)
                {
                    foreach (var m in messagesResult.Messages)
                    {
                        if (cmdArguments.ShowFullItem || messagesResult.RequireConfirmation)
                        {
                            Console.WriteLine($"{ m.Id }:{ m?.Value ?? "<null>"}");
                        }
                        else
                        {
                            Console.WriteLine(m?.Value ?? "<null>");
                        }
                    }
                }
            }
            else if (cmdArguments.Command == "confirm" || cmdArguments.Command == "confirmdequeue")
            {
                Console.WriteLine($"Result: { await client.ConfirmDequeueAsync(cmdArguments.MessageId) }");
            }
            else if (cmdArguments.Command == "length")
            {
                var lengthResult = await client.LengthAsync();
                if (cmdArguments.ShowFullItem && lengthResult.UnconfirmedItems.HasValue)
                {
                    Console.WriteLine($"{ lengthResult.QueueLength } (+{ lengthResult.UnconfirmedItems.Value } unconfirmed)");
                }
                else
                {
                    Console.WriteLine(lengthResult.QueueLength);
                }
            }
            else if (cmdArguments.Command == "register")
            {
                var queueProperties = await client.RegisterAsync(
                    lifetimeSeconds: cmdArguments.LifetimeSeconds,
                    itemLifetimeSeconds: cmdArguments.ItemLifetimeSeconds,
                    confirmationPeriodSeconds: cmdArguments.ConfirmationPeridSeconds,
                    suspendDequeue: cmdArguments.SuspendDequeue,
                    suspendEnqueue: cmdArguments.SuspendEnqueue);

                Console.WriteLine("registered queue properties:");
                Console.WriteLine("----------------------------");
                foreach (var propertyInfo in typeof(QueueProperties).GetProperties())
                {
                    Console.WriteLine($"{ propertyInfo.Name }: { propertyInfo.GetValue(queueProperties) }");
                }
            }
            else if (cmdArguments.Command == "properties")
            {
                var queueProperties = await client.PropertiesAsync();

                foreach (var propertyInfo in typeof(QueueProperties).GetProperties())
                {
                    Console.WriteLine($"{ propertyInfo.Name }: { propertyInfo.GetValue(queueProperties) }");
                }
            }
            else if (cmdArguments.Command == "queuenames")
            {
                foreach (var name in await client.QueueNamesAsync())
                {
                    Console.WriteLine(name);
                }
            }
            else if (cmdArguments.Command == "all")
            {
                var messagesResult = await client.AllMessagesAsync(cmdArguments.Max, cmdArguments.UnconfirmedOnly);
                if (messagesResult?.Messages != null)
                {
                    foreach (var m in messagesResult.Messages)
                    {
                        if (cmdArguments.ShowFullItem)
                        {
                            Console.WriteLine($"{ m.Id }:{ m?.Value ?? "<null>"}");
                        }
                        else
                        {
                            Console.WriteLine(m?.Value ?? "<null>");
                        }
                    }
                }

                if ((cmdArguments.ShowFullItem || cmdArguments.UnconfirmedOnly)  &&
                    messagesResult.UnconfirmedMessages != null &&
                    messagesResult.UnconfirmedMessages.Count() > 0)
                {
                    Console.WriteLine("Dequeued unconfirmed messages:");
                    foreach (var m in messagesResult.UnconfirmedMessages)
                    {
                        Console.WriteLine($"{ m.Id }:{ m?.Value ?? "<null>"}");
                    }
                }
            }
            else
            {
                throw new Exception($"Unknown command: { cmdArguments.Command  }");
            }
        }

        async static private Task Shell(string serverUrl)
        {
            Console.WriteLine("MessageQueueNET Shell");
            Console.WriteLine("Type help for help...");

            while (true)
            {
                Console.Write(">> ");
                var line = Console.ReadLine();

                if (String.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var args = Regex.Matches(line, @"[\""].+?[\""]|[^ ]+")
                    .Cast<Match>()
                    .Select(m =>
                    {
                        var val = m.Value.Trim();
                        if (val.StartsWith("\"") && val.EndsWith("\""))
                        {
                            val = val.Substring(1, val.Length - 2);
                        }

                        return val;
                    })
                    .ToArray();

                //Console.WriteLine("args: [\n" + String.Join("\n", args) + "\n]");

                CmdArguments cmdArguments = null;

                if (args.Length == 1 && (args[0] == "exit" || args[0] == "quit"))
                {
                    return;
                }
                if (args.Length == 1 && args[0] == "version")
                {
                    Console.WriteLine(Version);
                    continue;
                }
                else if (args.Length == 1 && args[0] == "help")
                {
                    WriteShellHelp();
                    continue;
                }
                else if (args.Length == 1 && args[0] == "queuenames")
                {
                    cmdArguments = new CmdArguments()
                    {
                        ServerUrl = serverUrl,
                        Command = args[0]
                    };
                }
                else if (args.Length < 2)
                {
                    WriteShellHelp();
                    continue;
                }
                else
                {
                    cmdArguments = new CmdArguments()
                    {
                        ServerUrl = serverUrl,
                        QueueName = args[0],
                        Command = args[1]
                    };
                }
                try
                {
                    cmdArguments.Parse(args, 2);

                    await Exec(cmdArguments);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: { ex.Message }");
                }
            }
        }

        public static string Version =>
            Assembly
            .GetAssembly(typeof(MessageQueueNET.Client.QueueClient))
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            .InformationalVersion;
    }
}
