using System.Runtime.Serialization;

namespace bot.src.Brokers.Bingx;

[Serializable]
public class CloseAllPositionsException : Exception
{
    public CloseAllPositionsException() { }

    public CloseAllPositionsException(string message) : base(message) { }

    public CloseAllPositionsException(string message, Exception innerException) : base(message, innerException) { }

    protected CloseAllPositionsException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
