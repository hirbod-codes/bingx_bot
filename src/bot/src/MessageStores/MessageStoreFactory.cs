using bot.src.Data;
using bot.src.MessageStores.Gmail;
using bot.src.MessageStores.Gmail.Models;
using bot.src.MessageStores.InMemory;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace bot.src.MessageStores;

public class MessageStoreFactory : IMessageStoreFactory
{
    private readonly IConfigurationRoot _configuration;
    private readonly ILogger _logger;
    private readonly IMessageRepository _messageRepository;

    public MessageStoreFactory(IConfigurationRoot configuration, ILogger logger, IMessageRepository messageRepository)
    {
        _configuration = configuration;
        _logger = logger;
        _messageRepository = messageRepository;
    }

    public IMessageStore CreateMessageStore() => _configuration[ConfigurationKeys.MESSAGE_STORE_NAME]! switch
    {
        "InMemory" => new MessageStore(_messageRepository),
        "Gmail" => new GmailMessageStore(new MessageProviderOptions()
        {
            ClientId = _configuration[$"{ConfigurationKeys.MESSAGE_STORE_OPTIONS}:{_configuration[ConfigurationKeys.MESSAGE_STORE_NAME]!}:{_configuration[ConfigurationKeys.GMAIL_PROVIDER_NAME]!}:ClientId"]!,
            ClientSecret = _configuration[$"{ConfigurationKeys.MESSAGE_STORE_OPTIONS}:{_configuration[ConfigurationKeys.MESSAGE_STORE_NAME]!}:{_configuration[ConfigurationKeys.GMAIL_PROVIDER_NAME]!}:ClientSecret"]!,
            DataStoreFolderAddress = _configuration[$"{ConfigurationKeys.MESSAGE_STORE_OPTIONS}:{_configuration[ConfigurationKeys.MESSAGE_STORE_NAME]!}:{_configuration[ConfigurationKeys.GMAIL_PROVIDER_NAME]!}:DataStoreFolderAddress"]!,
            OwnerGmail = _configuration[$"{ConfigurationKeys.MESSAGE_STORE_OPTIONS}:{_configuration[ConfigurationKeys.MESSAGE_STORE_NAME]!}:{_configuration[ConfigurationKeys.GMAIL_PROVIDER_NAME]!}:OwnerGmail"]!,
            SignalProviderEmail = _configuration[$"{ConfigurationKeys.MESSAGE_STORE_OPTIONS}:{_configuration[ConfigurationKeys.MESSAGE_STORE_NAME]!}:{_configuration[ConfigurationKeys.GMAIL_PROVIDER_NAME]!}:SignalProviderEmail"]!
        }, _logger),
        _ => throw new Exception()
    };
}
