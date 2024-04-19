using System.Runtime.Serialization;
using Abstractions.src.Data.Exceptions;

namespace Data.src.InMemory.Exceptions;

[Serializable]
public class CancellingAPositionException : DataException
{
    public CancellingAPositionException()
    {
    }

    public CancellingAPositionException(string? message) : base(message)
    {
    }

    public CancellingAPositionException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected CancellingAPositionException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
