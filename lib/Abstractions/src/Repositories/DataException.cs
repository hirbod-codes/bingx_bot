using System.Runtime.Serialization;

namespace Abstractions.src.Data.Exceptions;

[Serializable]
public class DataException : Exception
{
    public DataException()
    {
    }

    public DataException(string? message) : base(message)
    {
    }

    public DataException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected DataException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
