using System.Runtime.Serialization;

namespace bot.src.Data.InMemory;

[Serializable]
public class PositionCreationException : Exception
{
    public PositionCreationException()
    {
    }

    public PositionCreationException(string? message) : base(message)
    {
    }

    public PositionCreationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected PositionCreationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
