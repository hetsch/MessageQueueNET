using System.Collections.Generic;

namespace MessageQueueNET.Client.Models
{
    public class QueueLengthResult : ApiResult
    {
        public Dictionary<string, QueueLengthItem>? Queues { get; set; }
    }

    public class QueueLengthItem
    {
        public int QueueLength { get; set; }
        public int? UnconfirmedItems { get; set; }
    }
}
