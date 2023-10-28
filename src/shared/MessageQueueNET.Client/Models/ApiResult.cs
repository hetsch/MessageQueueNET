using System;
using System.Collections.Generic;
using System.Text;

namespace MessageQueueNET.Client.Models;

public class ApiResult
{
    public ApiResult() { }

    public ApiResult(int hashCode)
    {
        this.HashCode = hashCode;
    }

    public bool Success { get; set; } = true;
    public string? ErrorMessage { get; set; }

    public int? HashCode { get; set; }
}
