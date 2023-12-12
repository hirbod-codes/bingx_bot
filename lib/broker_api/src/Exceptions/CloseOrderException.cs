using System.Runtime.Serialization;

namespace broker_api.Exceptions;

[Serializable]
public class CloseOrderException : BingxApiException
{
    public CloseOrderException()
    {
    }

    public CloseOrderException(string? message) : base(message)
    {
    }

    public CloseOrderException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected CloseOrderException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
