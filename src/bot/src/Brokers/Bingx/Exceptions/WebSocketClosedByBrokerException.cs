using System.Runtime.Serialization;

namespace bot.src.Brokers.Bingx.Exceptions;

[Serializable]
public class WebSocketClosedByBrokerException : BrokerException
{
    public WebSocketClosedByBrokerException() { }

    public WebSocketClosedByBrokerException(string message) : base(message) { }

    public WebSocketClosedByBrokerException(string message, Exception innerException) : base(message, innerException) { }

    protected WebSocketClosedByBrokerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
