using System.Runtime.Serialization;

namespace bot.src.Brokers.Bingx;

[Serializable]
public class BingxServerTimeException : BrokerException
{
    public BingxServerTimeException()
    {
    }

    public BingxServerTimeException(string message) : base(message)
    {
    }

    public BingxServerTimeException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected BingxServerTimeException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
