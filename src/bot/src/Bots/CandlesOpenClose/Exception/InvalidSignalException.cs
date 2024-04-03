using System.Runtime.Serialization;

namespace bot.src.Bots.CandlesOpenClose;

[Serializable]
public class InvalidSignalException : BotException
{
    public InvalidSignalException() { }

    public InvalidSignalException(string message) : base(message) { }

    public InvalidSignalException(string message, Exception innerException) : base(message, innerException) { }

    protected InvalidSignalException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
