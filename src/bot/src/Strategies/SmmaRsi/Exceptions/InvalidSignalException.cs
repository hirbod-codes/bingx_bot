namespace bot.src.Strategies.SmmaRsi.Exceptions;

[System.Serializable]
public class InvalidSignalException : SmmaRsiStrategyException
{
    public InvalidSignalException() { }
    public InvalidSignalException(string message) : base(message) { }
    public InvalidSignalException(string message, System.Exception inner) : base(message, inner) { }
    protected InvalidSignalException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
