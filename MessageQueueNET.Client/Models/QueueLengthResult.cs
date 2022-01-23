using System;
using System.Collections.Generic;
using System.Text;

namespace MessageQueueNET.Client.Models
{
    public class QueueLengthResult
    {
        public int QueueLength { get; set; }
        public int? UnconfirmedItems { get; set; }
    }
}
