using System.Runtime.Serialization;

namespace gmail_api.Exceptions;

[Serializable]
public class GmailApiException : Exception
{
    public GmailApiException()
    {
    }

    public GmailApiException(string? message) : base(message)
    {
    }

    public GmailApiException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected GmailApiException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
