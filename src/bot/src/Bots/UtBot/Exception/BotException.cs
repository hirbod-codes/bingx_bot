using System.Runtime.Serialization;
using TheBotException = bot.src.Bots.BotException;

namespace bot.src.Bots.UtBot;

[Serializable]
public class BotException : TheBotException
{
    public BotException() { }

    public BotException(string message) : base(message) { }

    public BotException(string message, Exception innerException) : base(message, innerException) { }

    protected BotException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
