using MessageQueueNET.Core.Models;
using MessageQueueNET.Core.Services.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace MessageQueueNET.Client.Extensions;

static internal class QueueProcessorExteions
{
    static public bool ConfirmationRecommended(this IQueueProcessor? queueProcessor)
        => queueProcessor == null || queueProcessor.ConfirmAlways;
}
