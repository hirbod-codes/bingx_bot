using bot.src.MessageStores;
using Microsoft.Extensions.Configuration;
using Serilog;
using GeneralStrategyClass = bot.src.Strategies.GeneralStrategy.GeneralStrategy;

namespace bot.src.Strategies;

public class StrategyFactory : IStrategyFactory
{
    private readonly IConfigurationRoot _configuration;
    private readonly ILogger _logger;
    private readonly IMessageStoreFactory _messageStoreFactory;

    public StrategyFactory(IConfigurationRoot configuration, ILogger logger, IMessageStoreFactory messageStoreFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _messageStoreFactory = messageStoreFactory;
    }

    public IStrategy CreateStrategy() => _configuration[ConfigurationKeys.STRATEGY_NAME]! switch
    {
        "General" => new GeneralStrategyClass(_configuration[$"{ConfigurationKeys.STRATEGY_OPTIONS}:{_configuration[ConfigurationKeys.STRATEGY_NAME]!}:StrategyProvider"]!, _messageStoreFactory.CreateMessageStore(), _logger),
        _ => throw new Exception()
    };
}
