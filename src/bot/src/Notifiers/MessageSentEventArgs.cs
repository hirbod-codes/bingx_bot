using bot.src.MessageStores;

namespace bot.src.Notifiers;

public class MessageSentEventArgs
{
    public MessageSentEventArgs(IMessage message)
    {
        Message = message;
    }

    public IMessage Message { get; set; }
}
