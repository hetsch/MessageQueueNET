using MessageQueueNET.Client.Models;
using MessageQueueNET.Client.Models.Authentication;
using MessageQueueNET.Client.Services;

namespace MessageQueueNET.Client.Extensions;

static public class MessageQueueConnectionOptionsExtensions
{
    static public MessageQueueConnection ToConnection(this MessageQueueConnectionOptions options)
    {
        var connection = new MessageQueueConnection(options.MessageQueueApiUrl);

        if (!string.IsNullOrEmpty(options.MessageQueueClientId))
        {
            connection.Authentication = new BasicAuthentication(
                    options.MessageQueueClientId,
                    options.MessageQueueClientSecret
               );
        }

        return connection;
    }
}
