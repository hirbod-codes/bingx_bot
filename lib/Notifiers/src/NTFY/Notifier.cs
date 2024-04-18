using Abstractions.src.Notifiers;
using ILogger = Serilog.ILogger;

namespace Notifiers.src.NTFY;

public class Notifier : INotifier
{
    private readonly ILogger _logger;
    private readonly NotifierOptions _notifierOptions;

    public Notifier(INotifierOptions notifierOptions, ILogger logger)
    {
        _notifierOptions = (notifierOptions as NotifierOptions)!;
        _logger = logger.ForContext<Notifier>();
    }

    private void OnMessageSent(string message) => MessageSent?.Invoke(this, new MessageSentEventArgs(message));

    public event EventHandler<MessageSentEventArgs>? MessageSent;

    public async Task SendMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(_notifierOptions.Key))
        {
            _logger.Warning("Failure while trying to send notification, Key is not provided.");
            return;
        }

        _logger.Information("Sending notification...");
        _logger.Debug("Notification message: {message}", message);
        HttpResponseMessage response = await new HttpClient().PostAsync($"http://ntfy.sh/{_notifierOptions.Key}", new StringContent(message));
        _logger.Information("Notification has been sent...");

        _logger.Information("Raising MessageSent event...");
        OnMessageSent(message);
        _logger.Information("MessageSent event raised...");

        if (!response.IsSuccessStatusCode)
            _logger.Warning($"{nameof(Notifier)} failed to send a message, the message: {message}, the response:", message, await response.Content.ReadAsStringAsync());
    }
}
