using Abstractions.src.Strategies.Exceptions;

namespace Strategies.src.SuperTrendV1.Exceptions;

[System.Serializable]
public class SuperTrendStrategyException : StrategyException
{
    public SuperTrendStrategyException() { }
    public SuperTrendStrategyException(string message) : base(message) { }
    public SuperTrendStrategyException(string message, StrategyException inner) : base(message, inner) { }
    protected SuperTrendStrategyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
