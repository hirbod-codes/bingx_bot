using System.Runtime.Serialization;

namespace bingx_api.Exceptions;

[Serializable]
public class NotificationException : Exception
{
    public NotificationException()
    {
    }

    public NotificationException(string? message) : base(message)
    {
    }

    public NotificationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected NotificationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
