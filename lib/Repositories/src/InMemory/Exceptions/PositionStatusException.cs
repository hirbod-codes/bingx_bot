using System.Runtime.Serialization;
using Abstractions.src.Repository;

namespace Repositories.src.InMemory.Exceptions;

[Serializable]
public class PositionStatusException : DataException
{
    public PositionStatusException() { }

    public PositionStatusException(string message) : base(message) { }

    public PositionStatusException(string message, Exception innerException) : base(message, innerException) { }

    protected PositionStatusException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
