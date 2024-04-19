using Abstractions.src.MessageStores;

namespace Abstractions.src.Data;

public interface IMessageRepository
{
    public Task<IMessage> CreateMessage(IMessage message);
    public Task<IMessage> CreateMessage(string body, string from);
    public Task<IMessage?> GetMessage(string id);
    public Task<IMessage?> GetLastMessage();
    public Task<IMessage?> GetLastMessage(string from);
    public Task<IEnumerable<IMessage>> GetMessages();
    public Task<IEnumerable<IMessage>> GetMessages(IEnumerable<string> ids);
    public Task<IEnumerable<IMessage>> GetMessages(string from);
    public Task<bool> DeleteMessage(string id);
    public Task<bool> DeleteMessages(IEnumerable<string> ids);
    public Task<bool> DeleteMessages(string from);
}
