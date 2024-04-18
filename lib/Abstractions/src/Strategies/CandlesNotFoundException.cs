using System.Runtime.Serialization;

namespace Abstractions.src.Strategies;

[Serializable]
public class CandlesNotFoundException : StrategyException
{
    public CandlesNotFoundException()
    {
    }

    public CandlesNotFoundException(string message) : base(message)
    {
    }

    public CandlesNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected CandlesNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
