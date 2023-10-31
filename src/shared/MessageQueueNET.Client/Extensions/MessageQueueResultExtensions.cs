using MessageQueueNET.Client.Models;
using System;

namespace MessageQueueNET.Client.Extensions;
static internal class MessageQueueResultExtensions
{
    static public bool ConfirmationRequired(/*[NotNullWhen(true)]*/ this MessageResult? messageResult)
        => messageResult != null
           && messageResult.RequireConfirmation == true
           && !String.IsNullOrEmpty(messageResult.Queue);
}
