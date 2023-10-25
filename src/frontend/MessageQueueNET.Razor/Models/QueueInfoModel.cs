namespace MessageQueueNET.Razor.Models;

internal class QueueInfoModel
{
    public string QueueName { get; set; } = "";

    public DateTime LastAccessUTC { get; set; }

    public int QueueLength { get; set; }
    public int? UnconfirmedItems { get; set; }

    public bool SuspendEnqueue { get; set; }
    public bool SuspendDequeue { get; set; }

    public int LifetimeSeconds { get; set; }
    public int ItemLifetimeSeconds { get; set; }

    public int ConfirmationPeriodSeconds { get; set; }
}
