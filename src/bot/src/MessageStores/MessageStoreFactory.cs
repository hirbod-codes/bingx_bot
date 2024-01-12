using bot.src.MessageStores.Gmail;
using bot.src.MessageStores.Gmail.Models;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace bot.src.MessageStores;

public class MessageStoreFactory : IMessageStoreFactory
{
    private readonly IConfigurationRoot _configuration;
    private readonly ILogger _logger;

    public MessageStoreFactory(IConfigurationRoot configuration, ILogger logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public IMessageStore CreateMessageStore() => _configuration[ConfigurationKeys.MESSAGE_STORE_NAME]! switch
    {
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
