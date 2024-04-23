using System.Threading.Tasks;

namespace MessageQueueNET.Core.Services.Abstraction;
public interface IMessageHandler
{
    Task InvokeAsync(string message);
}
