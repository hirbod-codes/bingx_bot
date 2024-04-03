namespace bot.src.Strategies.EmaRsi.Exceptions;

[System.Serializable]
public class EmaRsiStrategyException : StrategyException
{
    public EmaRsiStrategyException() { }
    public EmaRsiStrategyException(string message) : base(message) { }
    public EmaRsiStrategyException(string message, System.Exception inner) : base(message, inner) { }
    protected EmaRsiStrategyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
