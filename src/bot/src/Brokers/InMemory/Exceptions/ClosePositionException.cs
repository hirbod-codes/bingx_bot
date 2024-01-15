using System.Runtime.Serialization;

namespace bot.src.Brokers.InMemory.Exceptions;

[Serializable]
internal class ClosePositionException : BrokerException
{
    public ClosePositionException() { }

    public ClosePositionException(string message) : base(message) { }

    public ClosePositionException(string message, Exception innerException) : base(message, innerException) { }

    protected ClosePositionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
