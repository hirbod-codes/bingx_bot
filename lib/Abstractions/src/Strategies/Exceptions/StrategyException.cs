namespace Abstractions.src.Strategies.Exceptions;

[System.Serializable]
public class StrategyException : System.Exception
{
    public StrategyException() { }
    public StrategyException(string message) : base(message) { }
    public StrategyException(string message, System.Exception inner) : base(message, inner) { }
    protected StrategyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
