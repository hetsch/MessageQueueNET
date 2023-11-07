namespace MessageQueueNET.Worker.Models.Process;

public class ProcessContext
{
    public string Command { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;

    public int ExitCode { get; set; }
    public string? Output { get; set; }
    public string? ErrorOutput { get; set; }
}
