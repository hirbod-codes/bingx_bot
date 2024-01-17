using bot.src.Data;
using bot.src.MessageStores.Gmail;
using bot.src.MessageStores.Gmail.Models;
using bot.src.MessageStores.InMemory;
using Serilog;

namespace bot.src.MessageStores;

public static class MessageStoreFactory
{
    public static IMessageStore CreateMessageStore(string MessageStoreName, MessageStoreOptions messageStoreOptions, ILogger logger) => MessageStoreName switch
    {
        "Gmail" => new GmailMessageStore(messageStoreOptions, logger),
        _ => throw new Exception()
    };

    public static IMessageStore CreateMessageStore(string MessageStoreName, IMessageRepository messageRepository, ILogger logger) => MessageStoreName switch
    {
        "InMemory" => new MessageStore(messageRepository, logger),
        _ => throw new Exception()
    };
}
