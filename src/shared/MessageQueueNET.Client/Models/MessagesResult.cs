using System.Collections.Generic;

namespace MessageQueueNET.Client.Models
{
    public class MessagesResult : ApiResult
    {
        public MessagesResult(){ }
        public MessagesResult(int hashCode) : base(hashCode) { }

        public IEnumerable<MessageResult>? Messages { get; set; }

        public IEnumerable<MessageResult>? UnconfirmedMessages { get; set; }
    }
}
