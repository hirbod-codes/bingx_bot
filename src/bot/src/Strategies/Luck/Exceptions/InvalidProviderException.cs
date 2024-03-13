namespace bot.src.Strategies.Luck.Exceptions;

[System.Serializable]
public class InvalidProviderException : LuckStrategyException
{
    public InvalidProviderException() { }
    public InvalidProviderException(string message) : base(message) { }
    public InvalidProviderException(string message, System.Exception inner) : base(message, inner) { }
    protected InvalidProviderException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
