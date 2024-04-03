using System.Runtime.Serialization;

namespace bot.src.Strategies.Luck.Exceptions;

[Serializable]
public class NoIndicatorException : Exception
{
    public NoIndicatorException() { }

    public NoIndicatorException(string message) : base(message) { }

    public NoIndicatorException(string message, Exception innerException) : base(message, innerException) { }

    protected NoIndicatorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
