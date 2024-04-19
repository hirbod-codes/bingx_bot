using System.Runtime.Serialization;

namespace Brokers.src.Bingx.Exceptions;

[Serializable]
public class MissingCandlesException : BingxException
{
    public MissingCandlesException()
    {
    }

    public MissingCandlesException(string message) : base(message)
    {
    }

    public MissingCandlesException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected MissingCandlesException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
