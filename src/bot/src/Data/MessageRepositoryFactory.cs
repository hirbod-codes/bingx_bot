using InMemoryMessageRepository =  bot.src.Data.InMemory.MessageRepository;
using NoneMessageRepository =  bot.src.Data.None.MessageRepository;

namespace bot.src.Data;

public static class MessageRepositoryFactory
{
    public static IMessageRepository CreateRepository(string repositoryType) => repositoryType switch
    {
        MessageRepositoryNames.IN_MEMORY => new InMemoryMessageRepository(),
        MessageRepositoryNames.NONE => new NoneMessageRepository(),
        _ => throw new Exception()
    };
}
