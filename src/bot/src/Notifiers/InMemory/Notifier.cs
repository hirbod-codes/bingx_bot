using bot.src.Data;
using bot.src.MessageStores;

namespace bot.src.Notifiers.InMemory;

public class Notifier : INotifier
{
    public event EventHandler<MessageSentEventArgs>? MessageSent;

    private readonly IMessageRepository _messageStoreRepository;

    public Notifier(IMessageRepository messageStoreRepository) => _messageStoreRepository = messageStoreRepository;

    public async Task SendMessage(IMessage message)
    {
        await _messageStoreRepository.CreateMessage(message);
        OnMessageSent(message);
    }

    private void OnMessageSent(IMessage message) => MessageSent?.Invoke(this, new MessageSentEventArgs(message));

    public async Task SendMessage(string message)
    {
        IMessage createdMessage = await _messageStoreRepository.CreateMessage(message);
        OnMessageSent(createdMessage);
    }
}
