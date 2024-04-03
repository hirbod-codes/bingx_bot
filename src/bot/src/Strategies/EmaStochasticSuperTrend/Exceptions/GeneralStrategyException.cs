namespace bot.src.Strategies.EmaStochasticSuperTrend.Exceptions;

[System.Serializable]
public class EmaStochasticSuperTrendStrategyException : StrategyException
{
    public EmaStochasticSuperTrendStrategyException() { }
    public EmaStochasticSuperTrendStrategyException(string message) : base(message) { }
    public EmaStochasticSuperTrendStrategyException(string message, System.Exception inner) : base(message, inner) { }
    protected EmaStochasticSuperTrendStrategyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
