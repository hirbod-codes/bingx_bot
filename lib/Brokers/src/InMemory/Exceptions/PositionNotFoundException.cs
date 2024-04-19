using System.Runtime.Serialization;

namespace Brokers.src.InMemory.Exceptions;

[Serializable]
public class PositionNotFoundException : BrokerException
{
    public PositionNotFoundException() { }

    public PositionNotFoundException(string message) : base(message) { }

    public PositionNotFoundException(string message, BrokerException innerException) : base(message, innerException) { }

    protected PositionNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
