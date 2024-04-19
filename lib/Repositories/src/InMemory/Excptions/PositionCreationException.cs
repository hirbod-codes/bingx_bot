using System.Data;
using System.Runtime.Serialization;

namespace Data.src.InMemory.Exceptions;

[Serializable]
public class PositionCreationException : DataException
{
    public PositionCreationException()
    {
    }

    public PositionCreationException(string? message) : base(message)
    {
    }

    public PositionCreationException(string? message, DataException? innerException) : base(message, innerException)
    {
    }

    protected PositionCreationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
