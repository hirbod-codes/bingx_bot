using System.Runtime.Serialization;
using AbstractBrokerException = bot.src.Brokers.BrokerException;

namespace bot.src.Brokers.InMemory.Exceptions;

public class BrokerException : AbstractBrokerException
{
    public BrokerException() { }

    public BrokerException(string message) : base(message) { }

    public BrokerException(string message, Exception innerException) : base(message, innerException) { }

    protected BrokerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
