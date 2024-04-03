namespace bot.src.Strategies.CandlesOpenClose.Exceptions;

[System.Serializable]
public class CandlesOpenCloseStrategyException : StrategyException
{
    public CandlesOpenCloseStrategyException() { }
    public CandlesOpenCloseStrategyException(string message) : base(message) { }
    public CandlesOpenCloseStrategyException(string message, System.Exception inner) : base(message, inner) { }
    protected CandlesOpenCloseStrategyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
