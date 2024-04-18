using System.Runtime.Serialization;

namespace Abstractions.src.Strategies;

[Serializable]
public class NotEnoughCandlesException : StrategyException
{
    public NotEnoughCandlesException() { }

    public NotEnoughCandlesException(string message) : base(message) { }

    public NotEnoughCandlesException(string message, Exception innerException) : base(message, innerException) { }

    protected NotEnoughCandlesException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
