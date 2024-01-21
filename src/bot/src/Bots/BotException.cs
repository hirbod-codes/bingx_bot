using System.Runtime.Serialization;

namespace bot.src.Bots;

[Serializable]
public class BotException : Exception
{
    public BotException() { }

    public BotException(string message) : base(message) { }

    public BotException(string message, Exception innerException) : base(message, innerException) { }

    protected BotException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
