using System.Runtime.Serialization;

namespace bot.src.Brokers.Bingx.Exceptions;

[Serializable]
public class SetLeverageException : Exception
{
    public SetLeverageException()
    {
    }

    public SetLeverageException(string? message) : base(message)
    {
    }

    public SetLeverageException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected SetLeverageException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
