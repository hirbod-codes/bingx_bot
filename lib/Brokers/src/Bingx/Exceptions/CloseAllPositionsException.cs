using System.Runtime.Serialization;

namespace Brokers.src.Bingx.Exceptions;

[Serializable]
public class CloseAllPositionsException : BingxException
{
    public CloseAllPositionsException() { }

    public CloseAllPositionsException(string message) : base(message) { }

    public CloseAllPositionsException(string message, Exception innerException) : base(message, innerException) { }

    protected CloseAllPositionsException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
