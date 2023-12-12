using System.Runtime.Serialization;

namespace email_api.Exceptions;

[Serializable]
public class EmailsDeletionException : GmailApiException
{
    public EmailsDeletionException()
    {
    }

    public EmailsDeletionException(string? message) : base(message)
    {
    }

    public EmailsDeletionException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected EmailsDeletionException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
