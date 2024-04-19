using System.Runtime.Serialization;

namespace Abstractions.src.Strategies.Exceptions;

[Serializable]
public class InvalidStrategyNameException : StrategyException
{
    public InvalidStrategyNameException() { }

    public InvalidStrategyNameException(string message) : base(message) { }

    public InvalidStrategyNameException(string message, StrategyException innerException) : base(message, innerException) { }

    protected InvalidStrategyNameException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
