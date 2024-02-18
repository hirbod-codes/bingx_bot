using bot;
using bot.src.Bots;
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
using bot.src.Brokers;
using BrokerOptionsFactory = StrategyTester.src.Brokers.BrokerOptionsFactory;
using BrokerFactory = StrategyTester.src.Brokers.BrokerFactory;

namespace StrategyTester;

public class Program
{
    private static async Task Main(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile("SmmaRsi.json")
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        ILogger logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration, new ConfigurationReaderOptions() { SectionName = ConfigurationKeys.SERILOG })
            .CreateLogger();

        IMessageStoreOptions messageStoreOptions = MessageStoreOptionsFactory.CreateMessageStoreOptions(configuration[ConfigurationKeys.MESSAGE_STORE_NAME]!);
        configuration.Bind($"{ConfigurationKeys.MESSAGE_STORE_OPTIONS}:{configuration[ConfigurationKeys.MESSAGE_STORE_NAME]}", messageStoreOptions);

        IBrokerOptions brokerOptions = BrokerOptionsFactory.CreateBrokerOptions(configuration[ConfigurationKeys.BROKER_NAME]!);
        configuration.Bind($"{ConfigurationKeys.BROKER_OPTIONS}:{configuration[ConfigurationKeys.BROKER_NAME]}", brokerOptions);

        IBotOptions botOptions = BotOptionsFactory.CreateBotOptions(configuration[ConfigurationKeys.BOT_NAME]!);
        configuration.Bind($"{ConfigurationKeys.BOT_OPTIONS}:{configuration[ConfigurationKeys.BOT_NAME]}", botOptions);

        IRiskManagementOptions riskManagementOptions = RiskManagementOptionsFactory.RiskManagementOptions(configuration[ConfigurationKeys.RISK_MANAGEMENT_NAME]!);
        configuration.Bind($"{ConfigurationKeys.RISK_MANAGEMENT_OPTIONS}:{configuration[ConfigurationKeys.RISK_MANAGEMENT_NAME]}", riskManagementOptions);

        IIndicatorOptions indicatorsOptions = IndicatorOptionsFactory.CreateIndicatorOptions(configuration[ConfigurationKeys.INDICATOR_OPTIONS_NAME]!);
        configuration.Bind($"{ConfigurationKeys.INDICATOR_OPTIONS}:{configuration[ConfigurationKeys.INDICATOR_OPTIONS_NAME]}", indicatorsOptions);

        IStrategyOptions strategyOptions = StrategyOptionsFactory.CreateStrategyOptions(configuration[ConfigurationKeys.STRATEGY_NAME]!);
        configuration.Bind($"{ConfigurationKeys.STRATEGY_OPTIONS}:{configuration[ConfigurationKeys.STRATEGY_NAME]}", strategyOptions);

        ITesterOptions testerOptions = TesterOptionsFactory.CreateTesterOptions(configuration[ConfigurationKeys.TESTER_NAME]!);
        configuration.Bind($"{ConfigurationKeys.TESTER_OPTIONS}:{configuration[ConfigurationKeys.TESTER_NAME]!}", testerOptions);

        ICandleRepository candleRepository = CandleRepositoryFactory.CreateRepository(configuration[ConfigurationKeys.CANDLE_REPOSITORY_TYPE]!);
        IPositionRepository positionRepository = PositionRepositoryFactory.CreateRepository(configuration[ConfigurationKeys.POSITION_REPOSITORY_TYPE]!);
        IMessageRepository messageRepository = MessageRepositoryFactory.CreateRepository(configuration[ConfigurationKeys.MESSAGE_REPOSITORY_TYPE]!);

        IMessageStore messageStore = MessageStoreFactory.CreateMessageStore(configuration[ConfigurationKeys.MESSAGE_STORE_NAME]!, messageStoreOptions, messageRepository, logger);

        ITime time = new Time();

        src.Brokers.IBroker broker = BrokerFactory.CreateBroker(configuration[ConfigurationKeys.BROKER_NAME]!, brokerOptions, positionRepository, time, logger);

        INotifier notifier = NotifierFactory.CreateNotifier(configuration[ConfigurationKeys.NOTIFIER_NAME]!, messageRepository, logger);

        IRiskManagement riskManagement = RiskManagementFactory.CreateRiskManager(configuration[ConfigurationKeys.RISK_MANAGEMENT_NAME]!, riskManagementOptions, broker, time);

        IStrategy strategy = StrategyFactory.CreateStrategy(configuration[ConfigurationKeys.STRATEGY_NAME]!, strategyOptions, indicatorsOptions, broker, notifier, messageRepository, logger);

        IBot bot = BotFactory.CreateBot(configuration[ConfigurationKeys.BOT_NAME]!, broker, botOptions, messageStore, riskManagement, time, notifier, logger);

        await TesterFactory.CreateTester(configuration[ConfigurationKeys.TESTER_NAME]!, testerOptions, time, strategy, broker, bot, logger).Test();

        AnalysisSummary analysisSummary = PnLAnalysis.RunAnalysis(await positionRepository.GetClosedPositions());

        string closedPositionsJson = JsonSerializer.Serialize(await positionRepository.GetClosedPositions());
        string analysisSummaryJson = JsonSerializer.Serialize(analysisSummary);

        await File.WriteAllTextAsync("./closed_positions.json", closedPositionsJson);
        await File.WriteAllTextAsync("./src/UI/Results/closed_positions.js", "var closedPositions = " + closedPositionsJson);
        await File.WriteAllTextAsync("./pnl_results.json", analysisSummaryJson);
        await File.WriteAllTextAsync("./src/UI/Results/pnl_results.js", "var pnlResults = " + analysisSummaryJson);
    }
}
