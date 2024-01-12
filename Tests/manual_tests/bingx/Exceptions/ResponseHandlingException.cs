using System.Runtime.Serialization;

namespace manual_tests.bingx.Exceptions;

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
