using System.Runtime.Serialization;

namespace bot.src.Brokers.Bingx.Exceptions;

[Serializable]
public class BingxException : BrokerException
{
    public BingxException() { }

    public BingxException(string message) : base(message) { }

    public BingxException(string message, Exception inner) : base(message, inner) { }

    protected BingxException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
