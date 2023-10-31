namespace MessageQueueNET.Processor;
internal class CommandLine
{
    public (string apiUrl, string filter)? Parse(string[] args)
    {
        string? apiUrl = null;
        string? filterPattern = null;

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

                default:
                    Console.Error.WriteLine($"unkonwn argument: {args[i]}");
                    break;
            }
        }

        if (apiUrl is not null && filterPattern is not null)
        {
            Console.WriteLine($"API URL: {apiUrl}");
            Console.WriteLine($"Filter Pattern: {filterPattern}");

            return (apiUrl, filterPattern);
        }
        else
        {
            Console.WriteLine("Missing Arguments. using: [--api|-a] <QueueApiUrl> [--filter|-f] <QueueNamePattern>");

            return null;
        }
    }
}
