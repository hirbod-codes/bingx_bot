using System.Text.Json;
using bot;
using bot.src.Bots;
using bot.src.Brokers;
using bot.src.Brokers.InMemory;
using bot.src.Data;
using bot.src.Data.InMemory;
using bot.src.Data.Models;
using bot.src.MessageStores;
using bot.src.Notifiers.InMemory;
using bot.src.PnLAnalysis;
using bot.src.PnLAnalysis.Models;
using bot.src.RiskManagement;
using bot.src.Strategies;
using Microsoft.Extensions.Configuration;
using providers.src;
using providers.src.Indicators;
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

        _logger = new LoggerConfiguration()
            .ReadFrom.Configuration(_configuration, new ConfigurationReaderOptions() { SectionName = ConfigurationKeys.SERILOG })
            .CreateLogger();

        BrokerOptions brokerOptions = new();
        _configuration.Bind($"{ConfigurationKeys.BROKER_OPTIONS}:{_configuration[ConfigurationKeys.BROKER_NAME]!}", brokerOptions);

        IndicatorsOptions indicatorsOptions = new();
        _configuration.Bind(ConfigurationKeys.INDICATORS_OPTIONS, indicatorsOptions);

        RiskManagementOptions riskManagementOptions = new();
        _configuration.Bind(ConfigurationKeys.RISK_MANAGEMENT_OPTIONS, riskManagementOptions);

        IMessageRepository messageRepository = new MessageRepository();
        ICandleRepository candleRepository = new CandleRepository();
        IPositionRepository positionRepository = new PositionRepository();

        IMessageStore messageStore = MessageStoreFactory.CreateMessageStore(_configuration[ConfigurationKeys.MESSAGE_STORE_NAME]!, messageRepository, _logger);
        Notifier notifier = new(messageRepository, _logger);

        ITime time = new Time();
        IStrategy strategy = StrategyFactory.CreateStrategy(_configuration[ConfigurationKeys.STRATEGY_NAME]!, messageStore, nameof(SmmaRsiStrategyProvider), _logger, time);

        IAccount account = BrokerFactory.CreateAccount(_configuration[ConfigurationKeys.BROKER_NAME]!, brokerOptions, _logger);
        ITrade trade = BrokerFactory.CreateTrade(_configuration[ConfigurationKeys.BROKER_NAME]!, brokerOptions, candleRepository, positionRepository, _logger);
        IBroker broker = BrokerFactory.CreateBroker(_configuration[ConfigurationKeys.BROKER_NAME]!, brokerOptions, trade, account, candleRepository, _logger);

        IBot bot = BotFactory.CreateBot(_configuration[ConfigurationKeys.BOT_NAME]!, broker, strategy, time, _logger);

        IRiskManagement riskManagement = new RiskManagement(riskManagementOptions);
        Candles candles = await candleRepository.GetCandles();
        Candles _indicatorsCandles = await candleRepository.GetIndicatorsCandles();
        SmmaRsiStrategyProvider smmaRsiStrategyProvider = new(candleRepository, indicatorsOptions, _indicatorsCandles.GetSmma(indicatorsOptions.Smma1.Period), _indicatorsCandles.GetSmma(indicatorsOptions.Smma2.Period), _indicatorsCandles.GetSmma(indicatorsOptions.Smma3.Period), _indicatorsCandles.GetRsi(indicatorsOptions.Rsi.Period), notifier, riskManagement, _logger, time);

        await smmaRsiStrategyProvider.Initiate();

        while (await smmaRsiStrategyProvider.TryMoveToNextCandle())
        {
            await broker.CandleClosed();
            await bot.Tick();
        }

        IEnumerable<Position> openedPositions = await positionRepository.GetOpenedPositions();
        IEnumerable<Position> closedPositions = await positionRepository.GetClosedPositions();
        IEnumerable<Position> cancelledPositions = await positionRepository.GetCancelledPositions();
        IEnumerable<Position> pendingPositions = await positionRepository.GetPendingPositions();

        IEnumerable<IMessage> messages = await messageRepository.GetMessages();

        StrategySummary strategySummary = await new PnLAnalysis(positionRepository).RunAnalysis();

        await File.WriteAllTextAsync("./closed_positions.json", JsonSerializer.Serialize(closedPositions));
        await File.WriteAllTextAsync("./pnl_results.json", JsonSerializer.Serialize(strategySummary));
    }
}
