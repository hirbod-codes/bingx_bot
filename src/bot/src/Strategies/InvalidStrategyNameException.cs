using System.Runtime.Serialization;

namespace bot.src.Strategies;

[Serializable]
internal class InvalidStrategyNameException : Exception
{
    public InvalidStrategyNameException() { }

    public InvalidStrategyNameException(string message) : base(message) { }

    public InvalidStrategyNameException(string message, Exception innerException) : base(message, innerException) { }

    protected InvalidStrategyNameException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
