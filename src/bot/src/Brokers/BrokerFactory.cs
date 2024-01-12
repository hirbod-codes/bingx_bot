using bot.src.Brokers.Bingx;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace bot.src.Brokers;

public class BrokerFactory : IBrokerFactory
{
    private IConfigurationRoot _configuration;
    private readonly ILogger _logger;

    public BrokerFactory(IConfigurationRoot configuration, ILogger logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public ITrade CreateTrader() => _configuration[ConfigurationKeys.BROKER_NAME]! switch
    {
        "Bingx" => new Trade(_configuration[$"{ConfigurationKeys.BROKER_OPTIONS}:{_configuration[ConfigurationKeys.BROKER_NAME]!}:BaseUrl"]!, _configuration[$"{ConfigurationKeys.BROKER_OPTIONS}:{_configuration[ConfigurationKeys.BROKER_NAME]!}:ApiKey"]!, _configuration[$"{ConfigurationKeys.BROKER_OPTIONS}:{_configuration[ConfigurationKeys.BROKER_NAME]!}:ApiSecret"]!, _configuration[$"{ConfigurationKeys.BROKER_OPTIONS}:{_configuration[ConfigurationKeys.BROKER_NAME]!}:Symbol"]!, new BingxUtilities(_logger), _logger),
        _ => throw new Exception()
    };
}
