using bot.src;
using bot.src.Bots;
using bot.src.Broker;
using bot.src.Broker.InMemory;
using bot.src.Brokers;
using bot.src.Brokers.InMemory;
using bot.src.Data;
using bot.src.Data.InMemory;
using bot.src.Data.Models;
using bot.src.MessageStores;
using bot.src.Notifiers.InMemory;
using bot.src.Strategies;
using bot.src.Util;
using Microsoft.Extensions.Configuration;
using providers.src;
using providers.src.Providers;
using Serilog;
using Serilog.Settings.Configuration;
using Skender.Stock.Indicators;

namespace providers;

public class Program
{
    private static IConfigurationRoot _configuration = null!;
    private static ILogger _logger = null!;

    private static async Task Main(string[] args)
    {
        _configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        BrokerOptions brokerOptions = new();
        _configuration.Bind($"Brokers:{_configuration["BrokerName"]!}", brokerOptions);
        IndicatorsOptions indicatorsOptions = new();
        _configuration.Bind("IndicatorsOptions", brokerOptions);
        RiskManagementOptions riskManagementOptions = new();
        _configuration.Bind("RiskManagementOptions", riskManagementOptions);

        _logger = new LoggerConfiguration()
            .ReadFrom.Configuration(_configuration, new ConfigurationReaderOptions() { SectionName = "Serilog" })
            .CreateLogger();

        IMessageRepository messageRepository = new MessageRepository();
        ICandleRepository candleRepository = new CandleRepository();
        IPositionRepository positionRepository = new PositionRepository();

        IMessageStoreFactory messageStoreFactory = new MessageStoreFactory(_configuration, _logger, messageRepository);
        Notifier notifier = new(messageRepository);

        IStrategyFactory strategyFactory = new StrategyFactory(_configuration, _logger, messageStoreFactory);
        IBrokerFactory brokerFactory = new BrokerFactory(_configuration, _logger);

        IAccount account = new Account(brokerOptions.AccountOptions);
        ITrade trade = new Trade(account, candleRepository, positionRepository, brokerOptions.Symbol);
        IBroker broker = new Broker(trade, candleRepository);

        IBot bot = new BotFactory(_configuration, _logger, strategyFactory, brokerFactory, new Time()).CreateBot();

        IRiskManagement riskManagement = new RiskManagement(riskManagementOptions);
        Candles candles = await candleRepository.GetCandles();
        SmmaRsiStrategyProvider smmaRsiStrategyProvider = new(candleRepository, indicatorsOptions, candles.GetSmma(indicatorsOptions.Smma1.Period), candles.GetSmma(indicatorsOptions.Smma2.Period), candles.GetSmma(indicatorsOptions.Smma3.Period), candles.GetRsi(indicatorsOptions.Rsi.Period), notifier, riskManagement, _logger);

        notifier.MessageSent += async (o, args) =>
        {
            await bot.Tick();
            await smmaRsiStrategyProvider.MoveToNextCandle();
        };

        smmaRsiStrategyProvider.CandleClosed += async (o, args) =>
        {
            await broker.CandleClosed(args.Candle);
        };

        broker.CandleProcessed += async (o, args) =>
        {
            await smmaRsiStrategyProvider.MoveToNextCandle();
        };

        await smmaRsiStrategyProvider.MoveToNextCandle();

        System.Console.ReadLine();
    }
}
