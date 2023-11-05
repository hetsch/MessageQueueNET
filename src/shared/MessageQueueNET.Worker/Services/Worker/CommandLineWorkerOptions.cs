using System;

namespace MessageQueueNET.Worker.Services.Worker;

public class CommandLineWorkerOptions
{
    public string[] CommandFilters { get; set; } = Array.Empty<string>();
}
