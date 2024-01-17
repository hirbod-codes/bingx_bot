using System.Runtime.Serialization;

namespace bot.src.Data.Models;

[Serializable]
public class ZeroTimeFrameException : Exception
{
    public ZeroTimeFrameException() { }

    public ZeroTimeFrameException(string message) : base(message) { }

    public ZeroTimeFrameException(string message, Exception innerException) : base(message, innerException) { }

    protected ZeroTimeFrameException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
