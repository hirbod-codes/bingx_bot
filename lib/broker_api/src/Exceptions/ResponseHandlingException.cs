using System.Runtime.Serialization;

namespace broker_api.Exceptions;

[Serializable]
public class ResponseHandlingException : BingxApiException
{
    public ResponseHandlingException()
    {
    }

    public ResponseHandlingException(string? message) : base(message)
    {
    }

    public ResponseHandlingException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected ResponseHandlingException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
