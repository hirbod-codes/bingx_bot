namespace bot.src.Strategies.StochasticEma.Exceptions;

[System.Serializable]
public class InvalidSignalException : StochasticEmaStrategyException
{
    public InvalidSignalException() { }
    public InvalidSignalException(string message) : base(message) { }
    public InvalidSignalException(string message, System.Exception inner) : base(message, inner) { }
    protected InvalidSignalException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
