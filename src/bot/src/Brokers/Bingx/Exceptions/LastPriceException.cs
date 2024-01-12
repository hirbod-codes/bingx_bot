using System.Runtime.Serialization;

namespace bot.src.Brokers.Bingx.Exceptions;

[Serializable]
public class LastPriceException : Exception
{
    public LastPriceException() { }

    public LastPriceException(string message) : base(message) { }

    public LastPriceException(string message, Exception innerException) : base(message, innerException) { }

    protected LastPriceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
