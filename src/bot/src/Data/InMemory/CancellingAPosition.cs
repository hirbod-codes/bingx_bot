using System.Runtime.Serialization;

namespace bot.src.Data.InMemory;

[Serializable]
public class CancellingAPosition : Exception
{
    public CancellingAPosition()
    {
    }

    public CancellingAPosition(string? message) : base(message)
    {
    }

    public CancellingAPosition(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected CancellingAPosition(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
