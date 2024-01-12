using bot.src.MessageStores.Models;

namespace bot.src.MessageStores;

public interface IMessageStore
{
    /// <exception cref="NullReferenceException"></exception>
    public Task<IEnumerable<Message>> GetMessages();
    /// <exception cref="NullReferenceException"></exception>
    public Task<IEnumerable<Message>> GetMessages(string from);
    /// <exception cref="NullReferenceException"></exception>
    public Task<Message?> GetLastMessage();
    /// <exception cref="NullReferenceException"></exception>
    public Task<Message?> GetLastMessage(string from);
    /// <exception cref="NullReferenceException"></exception>
    public Task<Message?> GetMessage(string id);
    /// <exception cref="NullReferenceException"></exception>
    public Task<Message?> GetMessage(string attribute, string value);
    public Task<bool> DeleteMessage(string id);
    public Task<bool> DeleteMessages(string from);
    public Task<bool> DeleteMessages(IEnumerable<string> ids);
}
