namespace bot.src.Strategies.StochasticEma.Exceptions;

[System.Serializable]
public class StochasticEmaStrategyException : StrategyException
{
    public StochasticEmaStrategyException() { }
    public StochasticEmaStrategyException(string message) : base(message) { }
    public StochasticEmaStrategyException(string message, System.Exception inner) : base(message, inner) { }
    protected StochasticEmaStrategyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
