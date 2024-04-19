using System.Data;
using System.Runtime.Serialization;

namespace Brokers.src.InMemory.Exceptions;

[Serializable]
public class ClosingAClosedPosition : DataException
{
    public ClosingAClosedPosition() { }

    public ClosingAClosedPosition(string message) : base(message) { }

    public ClosingAClosedPosition(string message, DataException innerException) : base(message, innerException) { }

    protected ClosingAClosedPosition(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
