using bot.src.Data;
using bot.src.MessageStores;
using Serilog;

namespace bot.src.Notifiers.InMemory;

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

    public async Task SendMessage(IMessage message)
    {
        _logger.Information("Sending the message: {@message}", message);
        await _messageRepository.CreateMessage(message);
        _logger.Information("The message sent.");

        OnMessageSent(message);
    }

    private void OnMessageSent(IMessage message)
    {
        _logger.Information("Raising OnMessageSent event.");
        MessageSent?.Invoke(this, new MessageSentEventArgs(message));
        _logger.Information("OnMessageSent event raised.");
    }

    public async Task SendMessage(string message)
    {
        IMessage createdMessage = await _messageRepository.CreateMessage(message);
        _logger.Information("Sending the message: {@message}", createdMessage);
        await _messageRepository.CreateMessage(createdMessage);
        _logger.Information("The message sent.");

        _logger.Information("Raising OnMessageSent event.");
        OnMessageSent(createdMessage);
        _logger.Information("OnMessageSent event raised.");
    }
}
