using System;
using System.Collections.Generic;
using System.Text;

namespace MessageQueueNET.Client.Models
{
    public class MessageResult
    {
        public string? Queue { get; set; }
        public Guid Id { get; set; }
        public string? Value { get; set; }

        public bool? RequireConfirmation { get; set; }
        public int? ConfirmationPeriod { get; set; }
    }
}
