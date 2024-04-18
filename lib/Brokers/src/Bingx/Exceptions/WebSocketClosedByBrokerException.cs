using System.Runtime.Serialization;

namespace Brokers.src.Bingx.Exceptions;

[Serializable]
public class WebSocketClosedByBrokerException : BingxException
{
    public WebSocketClosedByBrokerException() { }

    public WebSocketClosedByBrokerException(string message) : base(message) { }

    public WebSocketClosedByBrokerException(string message, Exception innerException) : base(message, innerException) { }

    protected WebSocketClosedByBrokerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
