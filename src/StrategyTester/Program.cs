using bot;
using bot.src.Bots;
using bot.src.Brokers;
using bot.src.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Settings.Configuration;
using bot.src.Strategies;
using StrategyTester.src.Testers;
using StrategyTester.src.Utils;
using bot.src.MessageStores;
using bot.src.Notifiers;
using bot.src.Indicators;
using bot.src.RiskManagement;
using bot.src.PnLAnalysis;
using bot.src.PnLAnalysis.Models;
using System.Text.Json;

namespace StrategyTester;

public class Program
{
    private static async Task Main(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        ILogger logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration, new ConfigurationReaderOptions() { SectionName = ConfigurationKeys.SERILOG })
            .CreateLogger();

        IMessageStoreOptions messageStoreOptions = MessageStoreOptionsFactory.CreateMessageStoreOptions(configuration[ConfigurationKeys.MESSAGE_STORE_NAME]!);
        configuration.Bind($"{configuration[ConfigurationKeys.MESSAGE_STORE_OPTIONS]}:{ConfigurationKeys.MESSAGE_STORE_NAME}", messageStoreOptions);

        IBrokerOptions brokerOptions = BrokerOptionsFactory.CreateBrokerOptions(configuration[ConfigurationKeys.BROKER_NAME]!);
        configuration.Bind($"{ConfigurationKeys.BROKER_OPTIONS}:{configuration[ConfigurationKeys.BROKER_NAME]}", brokerOptions);

        IBotOptions botOptions = BotOptionsFactory.CreateBotOptions(configuration[ConfigurationKeys.BOT_NAME]!);
        configuration.Bind($"{configuration[ConfigurationKeys.BOT_OPTIONS]}:{ConfigurationKeys.BOT_NAME}", botOptions);

        IRiskManagementOptions riskManagementOptions = RiskManagementOptionsFactory.RiskManagementOptions(configuration[ConfigurationKeys.RISK_MANAGEMENT_NAME]!);
        configuration.Bind($"{configuration[ConfigurationKeys.RISK_MANAGEMENT_OPTIONS]}:{ConfigurationKeys.RISK_MANAGEMENT_NAME}", riskManagementOptions);

        IIndicatorsOptions indicatorsOptions = IndicatorsOptionsFactory.CreateIndicatorOptions(configuration[ConfigurationKeys.INDICATORS_OPTIONS_NAME]!);
        configuration.Bind($"{configuration[ConfigurationKeys.INDICATORS_OPTIONS]}:{ConfigurationKeys.INDICATORS_OPTIONS_NAME}", indicatorsOptions);

        IStrategyOptions  strategyOptions = StrategyOptionsFactory.CreateStrategyOptions(configuration[ConfigurationKeys.STRATEGY_NAME]!);
        configuration.Bind($"{configuration[ConfigurationKeys.INDICATORS_OPTIONS]}:{ConfigurationKeys.STRATEGY_NAME}", strategyOptions);

        ICandleRepository candleRepository = CandleRepositoryFactory.CreateRepository(configuration[ConfigurationKeys.CANDLE_REPOSITORY_TYPE]!);
        IPositionRepository positionRepository = PositionRepositoryFactory.CreateRepository(configuration[ConfigurationKeys.POSITION_REPOSITORY_TYPE]!);
        IMessageRepository messageRepository = MessageRepositoryFactory.CreateRepository(configuration[ConfigurationKeys.MESSAGE_REPOSITORY_TYPE]!);

        IMessageStore messageStore = MessageStoreFactory.CreateMessageStore(configuration[ConfigurationKeys.MESSAGE_STORE_NAME]!, messageStoreOptions, messageRepository, logger);

        ITime time = new Time();

        IAccount account = BrokerFactory.CreateAccount(configuration[ConfigurationKeys.BROKER_NAME]!, brokerOptions, logger);
        ITrade trade = BrokerFactory.CreateTrade(configuration[ConfigurationKeys.BROKER_NAME]!, brokerOptions, candleRepository, positionRepository, logger);
        IBroker broker = BrokerFactory.CreateBroker(configuration[ConfigurationKeys.BROKER_NAME]!, brokerOptions, trade, account, candleRepository, logger);

        await BotFactory.CreateBot(configuration[ConfigurationKeys.BOT_NAME]!, broker, botOptions, messageStore, time, logger).Run();

        INotifier notifier = NotifierFactory.CreateNotifier(configuration[ConfigurationKeys.NOTIFIER_NAME]!, messageRepository, logger);

        IRiskManagement riskManagement = RiskManagementFactory.CreateRiskManager(configuration[ConfigurationKeys.RISK_MANAGEMENT_NAME]!, riskManagementOptions);

        IStrategy strategy = StrategyFactory.CreateStrategy(configuration[ConfigurationKeys.STRATEGY_NAME]!, candleRepository, strategyOptions, indicatorsOptions, notifier, riskManagement, logger);

        IBot bot = BotFactory.CreateBot(configuration[ConfigurationKeys.BOT_NAME]!, broker, botOptions, messageStore, time, logger);

        await TesterFactory.CreateTester(configuration[ConfigurationKeys.TESTER_NAME]!, positionRepository, candleRepository, time, strategy, broker, bot).Test();

        StrategySummary strategySummary = await new PnLAnalysis(positionRepository).RunAnalysis();

        await File.WriteAllTextAsync("./closed_positions.json", JsonSerializer.Serialize(await positionRepository.GetClosedPositions()));
        await File.WriteAllTextAsync("./pnl_results.json", JsonSerializer.Serialize(strategySummary));
    }
}
