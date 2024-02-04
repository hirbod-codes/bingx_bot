using bot.src.Bots;
using bot.src.Brokers;
using bot.src.Data;
using bot.src.MessageStores;
using bot.src.Notifiers;
using bot.src.RiskManagement;
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
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        ILogger logger = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration, new ConfigurationReaderOptions() { SectionName = ConfigurationKeys.SERILOG })
        .CreateLogger();

        try
        {
            ICandleRepository candleRepository = CandleRepositoryFactory.CreateRepository(configuration[ConfigurationKeys.CANDLE_REPOSITORY_TYPE]!);
            IPositionRepository positionRepository = PositionRepositoryFactory.CreateRepository(configuration[ConfigurationKeys.POSITION_REPOSITORY_TYPE]!);
            IMessageRepository messageRepository = MessageRepositoryFactory.CreateRepository(configuration[ConfigurationKeys.MESSAGE_REPOSITORY_TYPE]!);

            INotifier notifier = NotifierFactory.CreateNotifier(configuration[ConfigurationKeys.NOTIFIER_NAME]!, messageRepository, logger);

            try
            {
                IMessageStoreOptions messageStoreOptions = MessageStoreOptionsFactory.CreateMessageStoreOptions(configuration[ConfigurationKeys.MESSAGE_STORE_NAME]!);
                configuration.Bind($"{configuration[ConfigurationKeys.MESSAGE_STORE_OPTIONS]}:{configuration["GmailProviderName"]!}:{ConfigurationKeys.MESSAGE_STORE_NAME}", messageStoreOptions);

                IBrokerOptions brokerOptions = BrokerOptionsFactory.CreateBrokerOptions(configuration[ConfigurationKeys.BROKER_NAME]!);
                configuration.Bind($"{ConfigurationKeys.BROKER_OPTIONS}:{configuration[ConfigurationKeys.BROKER_NAME]}", brokerOptions);

                IBotOptions botOptions = BotOptionsFactory.CreateBotOptions(configuration[ConfigurationKeys.BOT_NAME]!);
                configuration.Bind($"{configuration[ConfigurationKeys.BOT_OPTIONS]}:{ConfigurationKeys.BOT_NAME}", messageStoreOptions);

                IRiskManagementOptions riskManagementOptions = RiskManagementOptionsFactory.RiskManagementOptions(configuration[ConfigurationKeys.RISK_MANAGEMENT_NAME]!);
                configuration.Bind($"{configuration[ConfigurationKeys.RISK_MANAGEMENT_OPTIONS]}:{ConfigurationKeys.RISK_MANAGEMENT_NAME}", riskManagementOptions);

                IAccount account = BrokerFactory.CreateAccount(configuration[ConfigurationKeys.BROKER_NAME]!, brokerOptions, logger);
                ITrade trade = BrokerFactory.CreateTrade(configuration[ConfigurationKeys.BROKER_NAME]!, brokerOptions, candleRepository, positionRepository, logger);
                IBroker broker = BrokerFactory.CreateBroker(configuration[ConfigurationKeys.BROKER_NAME]!, brokerOptions, trade, account, positionRepository, candleRepository, logger);

                IMessageStore messageStore = MessageStoreFactory.CreateMessageStore(configuration[ConfigurationKeys.MESSAGE_STORE_NAME]!, messageStoreOptions, messageRepository, logger);

                ITime time = new Time();

                IRiskManagement riskManagement = RiskManagementFactory.CreateRiskManager(configuration[ConfigurationKeys.RISK_MANAGEMENT_NAME]!, riskManagementOptions, broker, time);

                await BotFactory.CreateBot(configuration[ConfigurationKeys.BOT_NAME]!, broker, botOptions, messageStore, riskManagement, time, logger).Run();
            }
            catch (System.Exception ex)
            {
                logger.Error(ex, "An unhandled exception has been thrown.");
                try
                {
                    logger.Information(ex, "Notifying listeners...");
                    await notifier.SendMessage($"FATAL: Unhandled exception: {ex.Message}");
                    logger.Information(ex, "Listeners are notified.");
                }
                catch (System.Exception)
                {
                    logger.Information(ex, "Failed to notify listeners.");
                    throw;
                }
                return;
            }
        }
        catch (System.Exception ex)
        {
            logger.Error(ex, "An unhandled exception has been thrown.");
            return;
        }
    }
}
