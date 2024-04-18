using System.Runtime.Serialization;

namespace Strategies.src.SuperTrendV1.Exceptions;

[Serializable]
public class NoIndicatorException : SuperTrendStrategyException
{
    public NoIndicatorException() { }

    public NoIndicatorException(string message) : base(message) { }

    public NoIndicatorException(string message, Exception innerException) : base(message, innerException) { }

    protected NoIndicatorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
