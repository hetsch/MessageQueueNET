using MessageQueueNET.Core.Services.Abstraction;

namespace MessageQueueNET.Client.Extensions;

static internal class QueueProcessorExteions
{
    static public bool ConfirmationRecommended(this IQueueProcessor? queueProcessor)
        => queueProcessor == null || queueProcessor.ConfirmAlways;
}
