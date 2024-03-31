using bot.src.Bots;
using bot.src.Brokers;
using bot.src.Data;
using bot.src.Data.Models;
using bot.src.Indicators;
using bot.src.MessageStores;
using bot.src.Notifiers;
using bot.src.RiskManagement;
using bot.src.Runners;
using bot.src.Strategies;
using bot.src.Util;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Settings.Configuration;

namespace bot;

public class Program
{
    private static async Task Main(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddJsonFile("appsettings.json")
            .AddCommandLine(args)
            .Build();

        ILogger logger = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration, new ConfigurationReaderOptions() { SectionName = ConfigurationKeys.SERILOG })
        .CreateLogger();

    start:

        try
        {
            IPositionRepository positionRepository = PositionRepositoryFactory.CreateRepository(configuration[ConfigurationKeys.POSITION_REPOSITORY_TYPE]!);
            IMessageRepository messageRepository = MessageRepositoryFactory.CreateRepository(configuration[ConfigurationKeys.MESSAGE_REPOSITORY_TYPE]!);

            INotifier notifier = NotifierFactory.CreateNotifier(configuration[ConfigurationKeys.NOTIFIER_NAME]!, messageRepository, logger);

            try
            {
                IMessageStoreOptions messageStoreOptions = MessageStoreOptionsFactory.CreateMessageStoreOptions(configuration[ConfigurationKeys.MESSAGE_STORE_NAME]!);
                configuration.Bind($"{ConfigurationKeys.MESSAGE_STORE_OPTIONS}:{configuration[ConfigurationKeys.MESSAGE_STORE_NAME]!}", messageStoreOptions);

                IBrokerOptions brokerOptions = BrokerOptionsFactory.CreateBrokerOptions(configuration[ConfigurationKeys.BROKER_NAME]!);
                configuration.Bind($"{ConfigurationKeys.BROKER_OPTIONS}:{configuration[ConfigurationKeys.BROKER_NAME]!}", brokerOptions);

                IRiskManagementOptions riskManagementOptions = RiskManagementOptionsFactory.RiskManagementOptions(configuration[ConfigurationKeys.RISK_MANAGEMENT_NAME]!);
                configuration.Bind($"{ConfigurationKeys.RISK_MANAGEMENT_OPTIONS}:{configuration[ConfigurationKeys.RISK_MANAGEMENT_NAME]!}", riskManagementOptions);

                IIndicatorOptions indicatorOptions = IndicatorOptionsFactory.CreateIndicatorOptions(configuration[ConfigurationKeys.INDICATOR_OPTIONS_NAME]!);
                configuration.Bind($"{ConfigurationKeys.INDICATOR_OPTIONS}:{configuration[ConfigurationKeys.INDICATOR_OPTIONS_NAME]!}", indicatorOptions);

                IStrategyOptions strategyOptions = StrategyOptionsFactory.CreateStrategyOptions(configuration[ConfigurationKeys.STRATEGY_NAME]!);
                configuration.Bind($"{ConfigurationKeys.STRATEGY_OPTIONS}:{configuration[ConfigurationKeys.STRATEGY_NAME]!}", strategyOptions);

                IBotOptions botOptions = BotOptionsFactory.CreateBotOptions(configuration[ConfigurationKeys.BOT_NAME]!);
                configuration.Bind($"{ConfigurationKeys.BOT_OPTIONS}:{configuration[ConfigurationKeys.BOT_NAME]!}", botOptions);

                IRunnerOptions runnerOptions = RunnerOptionsFactory.CreateRunnerOptions(configuration[ConfigurationKeys.RUNNER_NAME]!);
                configuration.Bind($"{ConfigurationKeys.RUNNER_OPTIONS}:{configuration[ConfigurationKeys.RUNNER_NAME]!}", runnerOptions);

                ITime time = new Time();

                IBroker broker = BrokerFactory.CreateBroker(configuration[ConfigurationKeys.BROKER_NAME]!, brokerOptions, logger, time);

                IMessageStore messageStore = MessageStoreFactory.CreateMessageStore(configuration[ConfigurationKeys.MESSAGE_STORE_NAME]!, messageStoreOptions, messageRepository, logger);

                IRiskManagement riskManagement = RiskManagementFactory.CreateRiskManager(configuration[ConfigurationKeys.RISK_MANAGEMENT_NAME]!, riskManagementOptions, broker, time, logger);

                IStrategy strategy = StrategyFactory.CreateStrategy(configuration[ConfigurationKeys.STRATEGY_NAME]!, strategyOptions, indicatorOptions, riskManagement, broker, notifier, messageRepository, logger);

                IBot bot = BotFactory.CreateBot(configuration[ConfigurationKeys.BOT_NAME]!, broker, botOptions, messageStore, riskManagement, time, notifier, logger);

                IRunner runner = RunnerFactory.CreateRunner(configuration[ConfigurationKeys.RUNNER_NAME]!, runnerOptions, bot, broker, strategy, time, notifier, logger);

                await runner.Run();

                logger.Information("Runner terminated at: {dateTime}", time.GetUtcNow().ToString());
                await notifier.SendMessage($"Runner terminated at: {time.GetUtcNow()}");
            }
            catch (System.Exception ex)
            {
                logger.Error(ex, "An unhandled exception has been thrown.");
                try
                {
                    logger.Information("Notifying listeners...");
                    await notifier.SendMessage($"Unhandled exception was thrown: {ex.Message}");
                    logger.Information("Listeners are notified.");
                }
                catch (System.Exception)
                {
                    logger.Error(ex, "Failed to notify listeners.");
                    throw;
                }

                logger.Information("Restarting...");
                goto start;
            }
        }
        catch (System.Exception ex)
        {
            logger.Information("Runner terminated with a fatal error at: {dateTime}", DateTime.UtcNow.ToString());
            logger.Error(ex, "An unhandled exception has been thrown.");
            return;
        }
    }
}
