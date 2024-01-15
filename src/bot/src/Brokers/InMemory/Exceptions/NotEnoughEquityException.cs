using System.Runtime.Serialization;

namespace bot.src.Brokers.InMemory.Exceptions;

[Serializable]
public class NotEnoughEquityException : BrokerException
{
    public NotEnoughEquityException() { }

    public NotEnoughEquityException(string message) : base(message) { }

    public NotEnoughEquityException(string message, Exception innerException) : base(message, innerException) { }

    protected NotEnoughEquityException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
