using System.Runtime.Serialization;
using Abstractions.src.Repository;

namespace bot.src.Brokers.InMemory.Exceptions;

[Serializable]
public class PositionNotFoundException : DataException
{
    public PositionNotFoundException() { }

    public PositionNotFoundException(string message) : base(message) { }

    public PositionNotFoundException(string message, Exception innerException) : base(message, innerException) { }

    protected PositionNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
