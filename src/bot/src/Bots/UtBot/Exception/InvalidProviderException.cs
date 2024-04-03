using System.Runtime.Serialization;

namespace bot.src.Bots.UtBot;

[Serializable]
public class InvalidProviderException : BotException
{
    public InvalidProviderException() { }

    public InvalidProviderException(string message) : base(message) { }

    public InvalidProviderException(string message, Exception innerException) : base(message, innerException) { }

    protected InvalidProviderException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
