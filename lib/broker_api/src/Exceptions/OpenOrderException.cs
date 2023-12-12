using System.Runtime.Serialization;

namespace broker_api.Exceptions;

[Serializable]
public class OpenOrderException : BingxApiException
{
    public OpenOrderException()
    {
    }

    public OpenOrderException(string? message) : base(message)
    {
    }

    public OpenOrderException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected OpenOrderException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
