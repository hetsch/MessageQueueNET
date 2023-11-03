using System;

namespace MessageQueueNET.Client.Models;

public class InfoResult : ApiResult
{
    public Version Version { get; set; } = new Version();
}
