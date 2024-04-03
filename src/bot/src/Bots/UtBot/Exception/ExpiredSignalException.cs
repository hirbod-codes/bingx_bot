using System.Runtime.Serialization;

namespace bot.src.Bots.UtBot;

[Serializable]
public class ExpiredSignalException : BotException
{
    public ExpiredSignalException() { }

    public ExpiredSignalException(string message) : base(message) { }

    public ExpiredSignalException(string message, Exception innerException) : base(message, innerException) { }

    protected ExpiredSignalException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
