using InMemoryMessageRepository = bot.src.Data.InMemory.MessageRepository;
using NoneMessageRepository = bot.src.Data.None.MessageRepository;

namespace bot.src.Data;

public static class MessageRepositoryFactory
{
    public static IMessageRepository CreateRepository(string repositoryType) => repositoryType switch
    {
        MessageRepositoryNames.IN_MEMORY => new InMemoryMessageRepository(),
        MessageRepositoryNames.NONE => new NoneMessageRepository(),
        _ => throw new Exception()
    };

    public static Type? GetInstanceType(string? name) => name switch
    {
        null => null,
        MessageRepositoryNames.IN_MEMORY => typeof(InMemoryMessageRepository),
        MessageRepositoryNames.NONE => typeof(NoneMessageRepository),
        _ => throw new Exception()
    };
}
