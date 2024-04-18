using System.Runtime.Serialization;
using Abstractions.src.Repository;

namespace bot.src.Brokers.InMemory.Exceptions;

[Serializable]
public class ClosingAClosedPositionException : DataException
{
    public ClosingAClosedPositionException() { }

    public ClosingAClosedPositionException(string message) : base(message) { }

    public ClosingAClosedPositionException(string message, Exception innerException) : base(message, innerException) { }

    protected ClosingAClosedPositionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
