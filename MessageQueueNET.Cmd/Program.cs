using MessageQueue.NET.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MessageQueueNET.Cmd
{
    class Program
    {
        async static Task<int> Main(string[] args)
        {
            string serverUrl = String.Empty, queueName = String.Empty,
                command = String.Empty;

            var messages = new List<string>();

            if (args.Length > 0)
            {
                serverUrl = args[0];

                for (int i = 1; i < args.Length-1; i++)
                {
                    switch(args[i])
                    {
                        case "-q":
                            queueName = args[++i];
                            break;
                        case "-c":
                            command = args[++i];
                            break;
                        case "-m":
                            messages.Add(args[++i]);
                            break;
                        case "-f":
                            messages.AddRange(File.ReadAllLines(args[++i])
                                    .Select(l => l.Trim())
                                    .Where(l => !String.IsNullOrEmpty(l)));
                            break;
                    }
                }
            }
            if (String.IsNullOrEmpty(serverUrl) || 
                String.IsNullOrEmpty(queueName) ||
                String.IsNullOrEmpty(command))
            {
                Console.WriteLine("Usage: MessageQueueNET.Cmd.exe serviceUrl -q queueName -c comand {-m message | -f messages-file}");
                return 1;
            }

            try
            {
                var client = new QueueClient(serverUrl, queueName);
                if (command == "remove")
                {
                    if (!await client.Remove())
                        throw new Exception("Can't remove queue");
                }
                else if (command == "enqueue")
                {
                    if (!await client.Enqueue(messages))
                        throw new Exception($"Can't enqueue messages...");
                }
                else
                {
                    throw new Exception($"Unknown command: { command }");
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: { ex.Message }");
                return 1;
            }
        }
    }
}
