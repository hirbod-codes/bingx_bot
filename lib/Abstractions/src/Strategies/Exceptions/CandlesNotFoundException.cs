using System.Runtime.Serialization;

namespace Abstractions.src.Strategies.Exceptions;

[Serializable]
public class CandlesNotFoundException : StrategyException
{
    public CandlesNotFoundException()
    {
    }

    public CandlesNotFoundException(string message) : base(message)
    {
    }

    public CandlesNotFoundException(string message, StrategyException innerException) : base(message, innerException)
    {
    }

    protected CandlesNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
