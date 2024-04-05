using ILogger = Serilog.ILogger;

namespace bot.src.Notifiers.NTFY;

public class Notifier : INotifier
{
    public ILogger Logger { get; set; }

    public Notifier(ILogger logger) => Logger = logger.ForContext<Notifier>();
    private void OnMessageSent(string message) => MessageSent?.Invoke(this, new MessageSentEventArgs(message));

    public event EventHandler<MessageSentEventArgs>? MessageSent;

    public async Task SendMessage(string message)
    {
        Logger.Information("Sending notification...");
        Logger.Information("Notification message: {message}", message);
        HttpResponseMessage response = await new HttpClient().PostAsync("http://ntfy.sh/xpSQ6aicPmPB38VV1653rq", new StringContent(message));
        Logger.Information("Notification has been sent...");

        Logger.Information("Raising MessageSent event...");
        OnMessageSent(message);
        Logger.Information("MessageSent event raised...");

        if (!response.IsSuccessStatusCode) Logger.Warning($"{nameof(Notifier)} failed to send a message, the message: {message}, the response:", message, await response.Content.ReadAsStringAsync());
    }
}
