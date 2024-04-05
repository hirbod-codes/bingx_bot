using bot.src.Data;
using bot.src.MessageStores.Gmail;
using bot.src.MessageStores.Gmail.Models;
using bot.src.MessageStores.InMemory;
using ILogger = Serilog.ILogger;

namespace bot.src.MessageStores;

public static class MessageStoreFactory
{
    public static IMessageStore CreateMessageStore(string MessageStoreName, IMessageStoreOptions messageStoreOptions, IMessageRepository? messageRepository, ILogger logger) => MessageStoreName switch
    {
        MessageStoreNames.GMAIL => new GmailMessageStore(messageStoreOptions, logger),
        MessageStoreNames.IN_MEMORY => new MessageStore(messageRepository!, logger),
        _ => throw new Exception()
    };
}
