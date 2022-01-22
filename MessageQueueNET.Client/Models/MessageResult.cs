using System;
using System.Collections.Generic;
using System.Text;

namespace MessageQueueNET.Client.Models
{
    public class MessageResult
    {
        public Guid Id { get; set; }
        public string Value { get; set; }
    }
}
