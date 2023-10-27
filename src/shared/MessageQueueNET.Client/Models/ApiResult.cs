using System;
using System.Collections.Generic;
using System.Text;

namespace MessageQueueNET.Client.Models;

public class ApiResult
{
    public bool Success { get; set; } = true;
    public string? ErrorMessage { get; set; }
}
