using System.Runtime.Serialization;

namespace bot.src.Notifiers.NTFY;

[Serializable]
public class NotificationException : NotifierException
{
    public NotificationException() { }
    public NotificationException(string message) : base(message) { }
    public NotificationException(string message, Exception innerException) : base(message, innerException) { }
    protected NotificationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
