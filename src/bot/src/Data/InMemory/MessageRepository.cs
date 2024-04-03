using bot.src.MessageStores;
using bot.src.MessageStores.InMemory.Models;

namespace bot.src.Data.InMemory;

public class MessageRepository : IMessageRepository
{
    private IEnumerable<IMessage> _messages = Array.Empty<IMessage>();

    public Task<IMessage> CreateMessage(IMessage message)
    {
        message.Id = !_messages.Any() ? "0" : (int.Parse(_messages.Last().Id) + 1).ToString();
        _messages = _messages.Append(message);
        return Task.FromResult(message);
    }

    public Task<IMessage> CreateMessage(string body, string from)
    {
        IMessage message = new Message
        {
            Id = !_messages.Any() ? "0" : (int.Parse(_messages.Last().Id) + 1).ToString(),
            Body = body,
            From = from,
            SentAt = DateTime.UtcNow
        };
        _messages = _messages.Append(message);
        return Task.FromResult(message);
    }

    public Task<bool> DeleteMessage(string id)
    {
        _messages = _messages.Where(o => o.Id != id);
        return Task.FromResult(true);
    }

    public Task<bool> DeleteMessages(IEnumerable<string> ids)
    {
        _messages = _messages.Where(o => !ids.Contains(o.Id));
        return Task.FromResult(true);
    }

    public Task<bool> DeleteMessages(string from)
    {
        _messages = _messages.Where(o => o.From != from);
        return Task.FromResult(true);
    }

    public Task<IMessage?> GetLastMessage() => Task.FromResult(_messages.LastOrDefault());

    public Task<IMessage?> GetLastMessage(string from) => Task.FromResult(_messages.LastOrDefault(o => o.From == from));

    public Task<IMessage?> GetMessage(string id) => Task.FromResult(_messages.FirstOrDefault(o => o.Id == id));

    public Task<IEnumerable<IMessage>> GetMessages() => Task.FromResult(_messages);

    public Task<IEnumerable<IMessage>> GetMessages(IEnumerable<string> ids) => Task.FromResult(_messages.Where(o => ids.Contains(o.Id)));

    public Task<IEnumerable<IMessage>> GetMessages(string from) => Task.FromResult(_messages.Where(o => o.From == from));
}
