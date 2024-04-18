using System.Runtime.Serialization;

namespace Brokers.src.Bingx.Exceptions;

[Serializable]
public class BingxServerTimeException : BingxException
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
