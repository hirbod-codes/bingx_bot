namespace Strategies.src.SuperTrendV1.Exceptions;

[System.Serializable]
public class InvalidProviderException : SuperTrendStrategyException
{
    public InvalidProviderException() { }
    public InvalidProviderException(string message) : base(message) { }
    public InvalidProviderException(string message, SuperTrendStrategyException inner) : base(message, inner) { }
    protected InvalidProviderException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
