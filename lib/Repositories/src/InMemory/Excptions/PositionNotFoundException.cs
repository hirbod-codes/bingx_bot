using System.Runtime.Serialization;
using Abstractions.src.Data.Exceptions;

namespace Brokers.src.InMemory.Exceptions;

[Serializable]
public class PositionNotFoundException : DataException
{
    public PositionNotFoundException() { }

    public PositionNotFoundException(string message) : base(message) { }

    public PositionNotFoundException(string message, DataException innerException) : base(message, innerException) { }

    protected PositionNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
