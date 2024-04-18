using System.Runtime.Serialization;

namespace Abstractions.src.Strategies;

[Serializable]
public class InvalidStrategyNameException : StrategyException
{
    public InvalidStrategyNameException() { }

    public InvalidStrategyNameException(string message) : base(message) { }

    public InvalidStrategyNameException(string message, Exception innerException) : base(message, innerException) { }

    protected InvalidStrategyNameException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
