using System.Runtime.Serialization;

namespace manual_tests.bingx.Exceptions;

[Serializable]
public class BingxApiException : Exception
{
    public BingxApiException()
    {
    }

    public BingxApiException(string? message) : base(message)
    {
    }

    public BingxApiException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected BingxApiException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
