using System.Runtime.Serialization;

namespace MessageQueueNET.Client;

// Never Change Values
public enum RemoveType
{
    Queue = 0,
    Messages = 1,
    UnconfirmedMessages = 2
}

// Never Change Values
public enum MaxUnconfirmedItemsStrategy
{
    [EnumMember(Value = "Absolute")]
    Absolute = 0,
    [EnumMember(Value = "PerClient")]
    PerClient = 1
}