namespace bot.src.Strategies.EmaStochasticSuperTrend.Exceptions;

[System.Serializable]
public class InvalidProviderException : EmaStochasticSuperTrendStrategyException
{
    public InvalidProviderException() { }
    public InvalidProviderException(string message) : base(message) { }
    public InvalidProviderException(string message, System.Exception inner) : base(message, inner) { }
    protected InvalidProviderException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
