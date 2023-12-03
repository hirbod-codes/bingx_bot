using System.Runtime.Serialization;

namespace bingx_api.Exceptions;

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
