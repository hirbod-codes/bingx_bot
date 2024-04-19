namespace Strategies.src.SuperTrendV1.Exceptions;

[System.Serializable]
public class InvalidSignalException : SuperTrendStrategyException
{
    public InvalidSignalException() { }
    public InvalidSignalException(string message) : base(message) { }
    public InvalidSignalException(string message, SuperTrendStrategyException inner) : base(message, inner) { }
    protected InvalidSignalException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
