namespace bot.src.Notifiers;

public class MessageSentEventArgs
{
    public MessageSentEventArgs(string message)
    {
        Message = message;
    }

    public string Message { get; set; }
}
