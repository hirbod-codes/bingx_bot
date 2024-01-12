namespace bot.src.Notifiers;

public interface INotifier
{
    public Task SendMessage(string message);
}
