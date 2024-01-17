using bot.src.Data;
using Serilog;

namespace bot.src.MessageStores.InMemory;

public class MessageStore : IMessageStore
{
    private readonly IMessageRepository _messageRepository;
    private readonly ILogger _logger;

    public MessageStore(IMessageRepository messageRepository, ILogger logger)
    {
        _messageRepository = messageRepository;
        _logger = logger;
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
