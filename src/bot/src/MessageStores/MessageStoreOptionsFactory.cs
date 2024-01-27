using GmailMessageStoreOptions = bot.src.MessageStores.Gmail.Models.MessageStoreOptions;
using InMemoryMessageStoreOptions = bot.src.MessageStores.InMemory.Models.MessageStoreOptions;

namespace bot.src.MessageStores;

public static class MessageStoreOptionsFactory
{
    public static IMessageStoreOptions CreateMessageStoreOptions(string messageStoreName) => messageStoreName switch
    {
        MessageStoreNames.GMAIL => new GmailMessageStoreOptions(),
        MessageStoreNames.IN_MEMORY => new InMemoryMessageStoreOptions(),
        _ => throw new Exception()
    };
}
