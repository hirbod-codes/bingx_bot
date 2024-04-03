namespace bot.src.Strategies.CandlesOpenClose.Exceptions;

[System.Serializable]
public class ExpiredSignalException : CandlesOpenCloseStrategyException
{
    public ExpiredSignalException() { }
    public ExpiredSignalException(string message) : base(message) { }
    public ExpiredSignalException(string message, System.Exception inner) : base(message, inner) { }
    protected ExpiredSignalException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
