using MessageQueueNET.Client.Models.Authentication;

namespace MessageQueueNET.Client.Models;

public class MessageQueueConnection
{
    public MessageQueueConnection(string apiUrl)
    {
        ApiUrl = apiUrl;
    }

    public string ApiUrl { get; }

    public IAuthentication? Authentication { get; set; }
}
