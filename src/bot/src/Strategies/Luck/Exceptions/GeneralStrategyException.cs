namespace bot.src.Strategies.Luck.Exceptions;

[System.Serializable]
public class LuckStrategyException : StrategyException
{
    public LuckStrategyException() { }
    public LuckStrategyException(string message) : base(message) { }
    public LuckStrategyException(string message, System.Exception inner) : base(message, inner) { }
    protected LuckStrategyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
