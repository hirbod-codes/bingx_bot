using bot.src.Bots;
using bot.src.Brokers;
using bot.src.Data;
using bot.src.MessageStores;
using bot.src.Notifiers.NTFY;
using bot.src.Util;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Settings.Configuration;

namespace bot;

public class Program
{
    private static IConfigurationRoot _configuration = null!;
    private static ILogger _logger = null!;

    private static async Task Main(string[] args)
    {
        try
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            _logger = new LoggerConfiguration()
            .ReadFrom.Configuration(_configuration, new ConfigurationReaderOptions() { SectionName = ConfigurationKeys.SERILOG })
            .CreateLogger();

            IMessageStoreOptions messageStoreOptions = MessageStoreOptionsFactory.CreateMessageStoreOptions(_configuration[ConfigurationKeys.MESSAGE_STORE_NAME]!);
            _configuration.Bind($"{_configuration[ConfigurationKeys.MESSAGE_STORE_OPTIONS]}:{ConfigurationKeys.MESSAGE_STORE_NAME}", messageStoreOptions);

            IBrokerOptions brokerOptions = BrokerOptionsFactory.CreateBrokerOptions(_configuration[ConfigurationKeys.BROKER_NAME]!);
            _configuration.Bind($"{ConfigurationKeys.BROKER_OPTIONS}:{_configuration[ConfigurationKeys.BROKER_NAME]}", brokerOptions);

            IBotOptions botOptions = BotOptionsFactory.CreateBotOptions(_configuration[ConfigurationKeys.BOT_NAME]!);
            _configuration.Bind($"{_configuration[ConfigurationKeys.BOT_OPTIONS]}:{ConfigurationKeys.BOT_NAME}", messageStoreOptions);

            ICandleRepository candleRepository = CandleRepositoryFactory.CreateRepository(_configuration[ConfigurationKeys.CANDLE_REPOSITORY_TYPE]!);
            IPositionRepository positionRepository = PositionRepositoryFactory.CreateRepository(_configuration[ConfigurationKeys.POSITION_REPOSITORY_TYPE]!);
            IMessageRepository messageRepository = MessageRepositoryFactory.CreateRepository(_configuration[ConfigurationKeys.MESSAGE_REPOSITORY_TYPE]!);

            IAccount account = BrokerFactory.CreateAccount(_configuration[ConfigurationKeys.BROKER_NAME]!, brokerOptions, _logger);
            ITrade trade = BrokerFactory.CreateTrade(_configuration[ConfigurationKeys.BROKER_NAME]!, brokerOptions, candleRepository, positionRepository, _logger);
            IBroker broker = BrokerFactory.CreateBroker(_configuration[ConfigurationKeys.BROKER_NAME]!, brokerOptions, trade, account, positionRepository, candleRepository, _logger);

            IMessageStore messageStore = MessageStoreFactory.CreateMessageStore(_configuration[ConfigurationKeys.MESSAGE_STORE_NAME]!, messageStoreOptions, messageRepository, _logger);
            Notifier notifier = new(_logger);

            ITime time = new Time();

            await BotFactory.CreateBot(_configuration[ConfigurationKeys.BOT_NAME]!, broker, botOptions, messageStore, time, _logger).Run();
        }
        catch (System.Exception ex)
        {
            _logger.Error(ex, "An unhandled exception has been thrown.");
            try
            {
                _logger.Information(ex, "Notifying listeners...");
                await new Notifier(_logger).SendMessage($"FATAL: Unhandled exception: {ex.Message}");
                _logger.Information(ex, "Listeners are notified.");
            }
            catch (System.Exception)
            {
                _logger.Information(ex, "Failed to notify listeners.");
                throw;
            }
            return;
        }
    }
}
