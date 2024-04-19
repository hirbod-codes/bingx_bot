using System.Runtime.Serialization;

namespace Brokers.src.Bingx;

[Serializable]
public class InvalidOrderTypeException : Exception
{
    public InvalidOrderTypeException()
    {
    }

    public InvalidOrderTypeException(string? message) : base(message)
    {
    }

    public InvalidOrderTypeException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected InvalidOrderTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
