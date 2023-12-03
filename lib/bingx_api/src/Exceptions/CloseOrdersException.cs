using System.Runtime.Serialization;

namespace bingx_api.Exceptions;

[Serializable]
public class CloseOrdersException : Exception
{
    public CloseOrdersException()
    {
    }

    public CloseOrdersException(string? message) : base(message)
    {
    }

    public CloseOrdersException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected CloseOrdersException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
