namespace MessageQueueNET.Worker.Models.Worker;

internal class CommandLineWorkerMessage
{
    public string Command { get; set; } = "";
    public string Arguments { get; set; } = "";
}
