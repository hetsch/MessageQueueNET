using System.Collections.Generic;

namespace MessageQueueNET.Client.Models
{
    public class MessagesResult
    {
        public IEnumerable<MessageResult>? Messages { get; set; }

        public IEnumerable<MessageResult>? UnconfirmedMessages { get; set; }
    }
}
