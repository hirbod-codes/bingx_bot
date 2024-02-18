using System.Runtime.Serialization;

namespace StrategyTester.src.Brokers.InMemory.Exceptions;

[Serializable]
public class ClosingAClosedPosition : BrokerException
{
    public ClosingAClosedPosition() { }

    public ClosingAClosedPosition(string message) : base(message) { }

    public ClosingAClosedPosition(string message, BrokerException innerException) : base(message, innerException) { }

    protected ClosingAClosedPosition(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
