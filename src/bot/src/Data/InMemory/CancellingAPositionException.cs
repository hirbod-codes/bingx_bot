using System.Runtime.Serialization;

namespace bot.src.Data.InMemory;

[Serializable]
public class CancellingAPositionException : Exception
{
    public CancellingAPositionException()
    {
    }

    public CancellingAPositionException(string? message) : base(message)
    {
    }

    public CancellingAPositionException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected CancellingAPositionException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
