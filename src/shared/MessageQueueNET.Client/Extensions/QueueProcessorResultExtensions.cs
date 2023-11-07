using MessageQueueNET.Core.Models;

namespace MessageQueueNET.Client.Extensions;
static internal class QueueProcessorResultExtensions
{
    static public bool ConfirmationRecommended(this QueueProcessorResult? queueProcessorResult)
        => queueProcessorResult == null || queueProcessorResult.Succeeded;
}
