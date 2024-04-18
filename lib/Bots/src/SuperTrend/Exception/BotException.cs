using System.Runtime.Serialization;
using TheBotException = Abstractions.src.Bots.BotException;

namespace Bots.src.SuperTrendV1;

[Serializable]
public class BotException : TheBotException
{
    public BotException() { }

    public BotException(string message) : base(message) { }

    public BotException(string message, Exception innerException) : base(message, innerException) { }

    protected BotException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
