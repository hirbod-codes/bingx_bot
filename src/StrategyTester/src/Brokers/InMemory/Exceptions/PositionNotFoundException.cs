using System.Runtime.Serialization;
using bot.src.Brokers;

namespace StrategyTester.src.Brokers.InMemory.Exceptions;

[Serializable]
public class PositionNotFoundException : BrokerException
{
    public PositionNotFoundException() { }

    public PositionNotFoundException(string message) : base(message) { }

    public PositionNotFoundException(string message, Exception innerException) : base(message, innerException) { }

    protected PositionNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
