using System.Runtime.Serialization;

namespace bot.src.Broker.InMemory;

[Serializable]
public class ClosingAClosedPosition : Exception
{
    public ClosingAClosedPosition() { }

    public ClosingAClosedPosition(string message) : base(message) { }

    public ClosingAClosedPosition(string message, Exception innerException) : base(message, innerException) { }

    protected ClosingAClosedPosition(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
