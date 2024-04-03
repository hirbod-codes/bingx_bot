using System.Runtime.Serialization;

namespace bot.src.Data.InMemory;

[Serializable]
public class PositionStatusException : Exception
{
    public PositionStatusException() { }

    public PositionStatusException(string message) : base(message) { }

    public PositionStatusException(string message, Exception innerException) : base(message, innerException) { }

    protected PositionStatusException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
