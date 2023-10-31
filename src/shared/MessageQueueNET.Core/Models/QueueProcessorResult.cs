using System;
using System.Collections.Generic;
using System.Text;

namespace MessageQueueNET.Core.Models;
public class QueueProcessorResult
{
    public bool Succeeded { get; set; } = true;
    public bool AlwaysConfirm { get; set; } = true;
    public string? ErrorMessages { get; set; }
}
