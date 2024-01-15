using bot.src.MessageStores;

namespace bot.src.Notifiers;

public interface INotifier
{
    public event EventHandler<MessageSentEventArgs>? MessageSent;
    public Task SendMessage(IMessage message);
    public Task SendMessage(string message);
}
