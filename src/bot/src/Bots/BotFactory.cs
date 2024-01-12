using bot.src.Brokers;
using bot.src.Strategies;
using bot.src.Util;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace bot.src.Bots;

public class BotFactory
{
    private readonly IConfigurationRoot _configuration;
    private readonly ILogger _logger;
    private readonly IStrategyFactory _strategyFactory;
    private readonly IBrokerFactory _brokerFactory;
    private readonly ITime _time;

    public BotFactory(IConfigurationRoot configuration, ILogger logger, IStrategyFactory strategyFactory, IBrokerFactory brokerFactory, ITime time)
    {
        _configuration = configuration;
        _logger = logger;
        _strategyFactory = strategyFactory;
        _brokerFactory = brokerFactory;
        _time = time;
    }

    public IBot CreateBot() => _configuration[ConfigurationKeys.BOT_NAME]! switch
    {
        "General" => new GeneralBot(_strategyFactory.CreateStrategy(), _brokerFactory.CreateTrader(), _time, _logger),
        _ => throw new Exception()
    };
}
