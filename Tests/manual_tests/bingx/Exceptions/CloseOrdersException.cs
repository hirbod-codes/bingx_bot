using System.Runtime.Serialization;

namespace manual_tests.bingx.Exceptions;

[Serializable]
public class CloseOrdersException : BingxApiException
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
