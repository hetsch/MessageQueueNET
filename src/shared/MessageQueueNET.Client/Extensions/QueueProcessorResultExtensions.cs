using MessageQueueNET.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MessageQueueNET.Client.Extensions;
static internal class QueueProcessorResultExtensions
{
    static public bool ConfirmationRecommended(this QueueProcessorResult? queueProcessorResult)
        => queueProcessorResult == null || queueProcessorResult.Succeeded;
}
