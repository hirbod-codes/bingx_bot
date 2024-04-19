using Abstractions.src.Data;
using Abstractions.src.MessageStores;
using MessageStores.src.Gmail;
using MessageStores.src.Gmail.Models;
using MessageStores.src.InMemory;
using ILogger = Serilog.ILogger;

namespace MessageStores.src;

public static class MessageStoreFactory
{
    public static IMessageStore CreateMessageStore(string MessageStoreName, IMessageStoreOptions messageStoreOptions, IMessageRepository? messageRepository, ILogger logger) => MessageStoreName switch
    {
        MessageStoreNames.GMAIL => new GmailMessageStore(messageStoreOptions, logger),
        MessageStoreNames.IN_MEMORY => new MessageStore(messageRepository!, logger),
        _ => throw new Exception()
    };

    public static Type? GetInstanceType(string? name) => name switch
    {
        null => null,
        MessageStoreNames.GMAIL => typeof(GmailMessageStore),
        MessageStoreNames.IN_MEMORY => typeof(MessageStore),
        _ => throw new Exception()
    };
}
