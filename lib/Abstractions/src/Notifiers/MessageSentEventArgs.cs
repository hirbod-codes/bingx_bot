namespace Abstractions.src.Notifiers;

public class MessageSentEventArgs
{
    public MessageSentEventArgs(string message)
    {
        Message = message;
    }

    public string Message { get; set; }
}
