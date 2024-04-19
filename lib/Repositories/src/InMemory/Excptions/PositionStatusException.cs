using System.Runtime.Serialization;
using Abstractions.src.Data.Exceptions;

namespace Data.src.InMemory.Exceptions;

[Serializable]
public class PositionStatusException : DataException
{
    public PositionStatusException() { }

    public PositionStatusException(string message) : base(message) { }

    public PositionStatusException(string message, DataException innerException) : base(message, innerException) { }

    protected PositionStatusException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
