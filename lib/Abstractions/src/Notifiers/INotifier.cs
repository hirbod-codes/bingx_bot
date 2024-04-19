namespace Abstractions.src.Notifiers;

public interface INotifier
{
    public event EventHandler<MessageSentEventArgs>? MessageSent;
    public Task SendMessage(string message);
}
