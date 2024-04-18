using Abstractions.src.Notifiers;
using Abstractions.src.Repository;
using ILogger = Serilog.ILogger;

namespace Notifiers.src.InMemory;

public class Notifier : INotifier
{
    public event EventHandler<MessageSentEventArgs>? MessageSent;

    private readonly IMessageRepository _messageRepository;
    private readonly ILogger _logger;

    public Notifier(IMessageRepository messageRepository, ILogger logger)
    {
        _messageRepository = messageRepository;
        _logger = logger.ForContext<Notifier>();
    }

    public Task SendMessage(string message)
    {
        _logger.Information("Sending the message: {message}", message);
        _logger.Information("The message sent.");

        OnMessageSent(message);

        return Task.CompletedTask;
    }

    private void OnMessageSent(string message)
    {
        _logger.Information("Raising OnMessageSent event.");
        MessageSent?.Invoke(this, new MessageSentEventArgs(message));
        _logger.Information("OnMessageSent event raised.");
    }
}
