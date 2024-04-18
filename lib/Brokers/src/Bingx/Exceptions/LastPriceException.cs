using System.Runtime.Serialization;

namespace Brokers.src.Bingx.Exceptions;

[Serializable]
public class LastPriceException : BingxException
{
    public LastPriceException() { }

    public LastPriceException(string message) : base(message) { }

    public LastPriceException(string message, Exception innerException) : base(message, innerException) { }

    protected LastPriceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
