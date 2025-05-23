namespace Strategies.src.SuperTrendV1.Exceptions;

[System.Serializable]
public class ExpiredSignalException : SuperTrendStrategyException
{
    public ExpiredSignalException() { }
    public ExpiredSignalException(string message) : base(message) { }
    public ExpiredSignalException(string message, SuperTrendStrategyException inner) : base(message, inner) { }
    protected ExpiredSignalException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
