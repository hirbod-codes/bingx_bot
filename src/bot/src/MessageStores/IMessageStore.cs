
namespace bot.src.MessageStores;

public interface IMessageStore
{
    /// <exception cref="NullReferenceException"></exception>
    public Task<IEnumerable<IMessage>> GetMessages();
    /// <exception cref="NullReferenceException"></exception>
    public Task<IEnumerable<IMessage>> GetMessages(string from);
    /// <exception cref="NullReferenceException"></exception>
    public Task<IMessage?> GetLastMessage();
    /// <exception cref="NullReferenceException"></exception>
    public Task<IMessage?> GetLastMessage(string from);
    /// <exception cref="NullReferenceException"></exception>
    public Task<IMessage?> GetMessage(string id);
    /// <exception cref="NullReferenceException"></exception>
    public Task<IMessage?> GetMessage(string attribute, string value);
    public Task<bool> DeleteMessage(string id);
    public Task<bool> DeleteMessages(string from);
    public Task<bool> DeleteMessages(IEnumerable<string> ids);
}
