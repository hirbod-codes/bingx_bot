using System.Runtime.Serialization;

namespace Brokers.src.Bingx.Exceptions;

[Serializable]
public class OpenMarketOrderException : BingxException
{
    public OpenMarketOrderException() { }

    public OpenMarketOrderException(string message) : base(message) { }

    public OpenMarketOrderException(string message, Exception innerException) : base(message, innerException) { }

    protected OpenMarketOrderException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
