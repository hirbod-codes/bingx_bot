using bot.src.MessageStores.Gmail.Models;

namespace bot.src.MessageStores;

public static class MessageStoreOptionsFactory
{
    public static IMessageStoreOptions CreateMessageStoreOptions(string messageStoreName) => messageStoreName switch
    {
        MessageStoreNames.GMAIL => new MessageStoreOptions(),
        _ => throw new Exception()
    };
}
