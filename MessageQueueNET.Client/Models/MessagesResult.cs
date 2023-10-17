using System;
using System.Collections.Generic;
using System.Text;

namespace MessageQueueNET.Client.Models
{
    public class MessagesResult
    {
        public bool RequireConfirmation { get; set; }
        public int? ConfirmationPeriod { get; set; }

        public IEnumerable<MessageResult>? Messages { get; set; }

        public IEnumerable<MessageResult>? UnconfirmedMessages { get; set; }
    }
}
