using MessageQueueNET.Models;

namespace MessageQueueNET.Extensions;

static public class QueuePropertiesExtensions
{
    static public bool MaxUnconfirmedItemsIsRestricted(this QueueProperties properties)
        => properties.MaxUnconfirmedItems > 0;

    static public bool RequireConfirmation(this QueueProperties properties)
        => properties.ConfirmationPeriodSeconds > 0;
}
