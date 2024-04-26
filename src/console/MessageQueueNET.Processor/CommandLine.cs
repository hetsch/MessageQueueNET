namespace MessageQueueNET.Processor;
internal class CommandLine
{
    public (string apiUrl, string filter, string outputPath, string commandFilter)? Parse(string[] args)
    {
        string? apiUrl = null;
        string? filterPattern = null;
        string? outputPath = null;
        string? commandFilter = null;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-a":
                case "--api":
                    if (i + 1 < args.Length)
                    {
                        apiUrl = args[++i];
                    }
                    else
                    {
                        Console.Error.WriteLine("Error: API-URL missing.");
                    }

                    break;

                case "-f":
                case "--filter":
                    if (i + 1 < args.Length)
                    {
                        filterPattern = args[++i];
                    }
                    else
                    {
                        Console.Error.WriteLine("Error: filter missing.");
                    }

                    break;
                case "-o":
                case "--output":
                    if (i + 1 < args.Length)
                    {
                        outputPath = args[++i];
                    }
                    else
                    {
                        Console.Error.WriteLine("Error: output path missing.");
                    }

                    break;
                case "-c":
                case "--command":
                    if (i + 1 < args.Length)
                    {
                        commandFilter = args[++i];
                    }
                    else
                    {
                        Console.Error.WriteLine("Error: command filter missing.");
                    }

                    break;
                default:
                    Console.Error.WriteLine($"unkonwn argument: {args[i]}");
                    break;
            }
        }

        if (apiUrl is not null
            && filterPattern is not null
            && outputPath is not null
            && commandFilter is not null)
        {
            Console.WriteLine($"API URL        : {apiUrl}");
            Console.WriteLine($"Filter Pattern : {filterPattern}");
            Console.WriteLine($"Output Path    : {outputPath}");
            Console.WriteLine($"Command Filter : {commandFilter}");

            return (apiUrl, filterPattern, outputPath, commandFilter);
        }
        else
        {
            Console.WriteLine("Missing Arguments. using: [--api|-a] <QueueApiUrl> [--filter|-f] <QueueNamePattern>  [--output|-o] <Output Path Folder> [--command|-c] <Command Filter Pattern>");
            Console.WriteLine(@"  MessageQueueNET.Processor.exe --api https://localhost/mq --filter mq-processor.* --output c:\temp\mq-processor --command: c:\jobs\*.bat");

            return null;
        }
    }
}
