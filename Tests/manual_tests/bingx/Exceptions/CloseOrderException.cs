using System.Runtime.Serialization;

namespace manual_tests.bingx.Exceptions;

[Serializable]
public class CloseOrderException : BingxApiException
{
    public CloseOrderException()
    {
    }

    public CloseOrderException(string? message) : base(message)
    {
    }

    public CloseOrderException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected CloseOrderException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
