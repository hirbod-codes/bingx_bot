using bot.src.Data;

namespace bot.src.MessageStores.InMemory;

public class MessageStore : IMessageStore
{
    private readonly IMessageRepository _messageRepository;

    public MessageStore(IMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }

    public Task<bool> DeleteMessage(string id) => _messageRepository.DeleteMessage(id);

    public Task<bool> DeleteMessages(string from) => _messageRepository.DeleteMessages(from);

    public Task<bool> DeleteMessages(IEnumerable<string> ids) => _messageRepository.DeleteMessages(ids);

    public Task<IMessage?> GetLastMessage() => _messageRepository.GetLastMessage();

    public Task<IMessage?> GetLastMessage(string from) => _messageRepository.GetLastMessage(from);

    public Task<IMessage?> GetMessage(string id) => _messageRepository.GetMessage(id);

    public Task<IMessage?> GetMessage(string attribute, string value) => throw new NotImplementedException();

    public Task<IEnumerable<IMessage>> GetMessages() => _messageRepository.GetMessages();

    public Task<IEnumerable<IMessage>> GetMessages(string from) => _messageRepository.GetMessages(from);
}
