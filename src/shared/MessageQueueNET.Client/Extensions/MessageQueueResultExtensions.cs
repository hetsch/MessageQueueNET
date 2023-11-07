using MessageQueueNET.Client.Models;
using System;
using System.Diagnostics.CodeAnalysis;

namespace MessageQueueNET.Client.Extensions;
static internal class MessageQueueResultExtensions
{
    static public bool ConfirmationRequired([NotNullWhen(true)] this MessageResult? messageResult)
        => messageResult != null
           && messageResult.RequireConfirmation == true;

    static public int CurrentClientUnconfirmedCount(this QueueProperties properties)
        => properties.DequeuingClients?.TryGetValue(QueueClient.ClientIdentity, out var count) == true
        ? count
        : 0;

    static public int DequeueMaxRecommendation(this QueueProperties properties)
        => Math.Min(properties switch
        {
            { SuspendDequeue: true } => 0,
            { ConfirmationPeriodSeconds: > 0, MaxUnconfirmedItems: > 0 } => properties.MaxUnconfirmedItems.Value - properties.CurrentClientUnconfirmedCount(),
            _ => properties.Length
        }, properties.Length);
}
