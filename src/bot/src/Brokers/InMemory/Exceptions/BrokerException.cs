using System.Runtime.Serialization;

namespace bot.src.Brokers.InMemory.Exceptions;

public class BrokerException : Exception
{
    public BrokerException() { }

    public BrokerException(string message) : base(message) { }

    public BrokerException(string message, Exception innerException) : base(message, innerException) { }

    protected BrokerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
