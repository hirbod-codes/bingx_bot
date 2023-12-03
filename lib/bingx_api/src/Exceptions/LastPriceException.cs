using System.Runtime.Serialization;

namespace bingx_api.Exceptions;

[Serializable]
public class LastPriceException : BingxApiException
{
    public LastPriceException()
    {
    }

    public LastPriceException(string? message) : base(message)
    {
    }

    public LastPriceException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected LastPriceException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
