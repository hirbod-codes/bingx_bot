namespace bot.src.Strategies.GeneralStrategy;

[System.Serializable]
public class GeneralStrategyException : StrategyException
{
    public GeneralStrategyException() { }
    public GeneralStrategyException(string message) : base(message) { }
    public GeneralStrategyException(string message, System.Exception inner) : base(message, inner) { }
    protected GeneralStrategyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
