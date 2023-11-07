namespace MessageQueueNET.Core.Services.Abstraction;

public interface IQueueProcessor
{
    string WorkerId { get; }

    bool ConfirmAlways { get; }
}
