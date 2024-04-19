using System.Runtime.Serialization;

namespace Brokers.src.InMemory.Exceptions;

[Serializable]
public class ClosePositionException : BrokerException
{
    public ClosePositionException() { }

    public ClosePositionException(string message) : base(message) { }

    public ClosePositionException(string message, BrokerException innerException) : base(message, innerException) { }

    protected ClosePositionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
