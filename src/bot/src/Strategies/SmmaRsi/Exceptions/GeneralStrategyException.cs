namespace bot.src.Strategies.SmmaRsi.Exceptions;

[System.Serializable]
public class SmmaRsiStrategyException : StrategyException
{
    public SmmaRsiStrategyException() { }
    public SmmaRsiStrategyException(string message) : base(message) { }
    public SmmaRsiStrategyException(string message, System.Exception inner) : base(message, inner) { }
    protected SmmaRsiStrategyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
