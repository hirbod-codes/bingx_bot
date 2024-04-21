using Abstractions.src.MessageStores;
using GmailMessageStoreOptions = MessageStores.src.Gmail.Models.MessageStoreOptions;
using InMemoryMessageStoreOptions = MessageStores.src.InMemory.Models.MessageStoreOptions;

namespace MessageStores.src;

public static class MessageStoreOptionsFactory
{
    public static IMessageStoreOptions CreateMessageStoreOptions(string messageStoreName) => messageStoreName switch
    {
        MessageStoreNames.GMAIL => new GmailMessageStoreOptions(),
        MessageStoreNames.IN_MEMORY => new InMemoryMessageStoreOptions(),
        _ => throw new Exception()
    };

    public static Type? GetInstanceType(string? name) => name switch
    {
        null => null,
        MessageStoreNames.GMAIL => typeof(GmailMessageStoreOptions),
        MessageStoreNames.IN_MEMORY => typeof(InMemoryMessageStoreOptions),
        _ => throw new Exception()
    };
}
