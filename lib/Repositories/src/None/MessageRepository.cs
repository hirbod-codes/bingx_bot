using Abstractions.src.Models;
using Abstractions.src.Repository;

namespace Repositories.src.None;

public class MessageRepository : IMessageRepository
{
    public Task<IMessage> CreateMessage(IMessage message)
    {
        throw new NotImplementedException();
    }

    public Task<IMessage> CreateMessage(string body, string from)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteMessage(string id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteMessages(IEnumerable<string> ids)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteMessages(string from)
    {
        throw new NotImplementedException();
    }

    public Task<IMessage?> GetLastMessage()
    {
        throw new NotImplementedException();
    }

    public Task<IMessage?> GetLastMessage(string from)
    {
        throw new NotImplementedException();
    }

    public Task<IMessage?> GetMessage(string id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<IMessage>> GetMessages()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<IMessage>> GetMessages(IEnumerable<string> ids)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<IMessage>> GetMessages(string from)
    {
        throw new NotImplementedException();
    }
}
