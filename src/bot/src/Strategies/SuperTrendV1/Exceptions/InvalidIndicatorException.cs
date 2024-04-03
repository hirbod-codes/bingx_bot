namespace bot.src.Strategies.SuperTrendV1.Exceptions;

[System.Serializable]
public class InvalidIndicatorException : Exception
{
    public InvalidIndicatorException() { }

    public InvalidIndicatorException(string message) : base(message) { }

    public InvalidIndicatorException(string message, Exception innerException) : base(message, innerException) { }

    protected InvalidIndicatorException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
