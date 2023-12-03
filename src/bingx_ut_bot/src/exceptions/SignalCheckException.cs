using System.Runtime.Serialization;

namespace bingx_ut_bot.Exceptions;

[Serializable]
public class SignalCheckException : Exception
{
    public SignalCheckException()
    {
    }

    public SignalCheckException(string? message) : base(message)
    {
    }

    public SignalCheckException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected SignalCheckException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
