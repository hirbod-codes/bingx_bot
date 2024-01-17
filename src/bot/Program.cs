using bot.src.Bots;
using bot.src.Brokers;
using bot.src.Data;
using bot.src.Data.InMemory;
using bot.src.MessageStores;
using bot.src.MessageStores.Gmail.Models;
using bot.src.Notifiers.NTFY;
using bot.src.Strategies;
using bot.src.Util;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Settings.Configuration;
using BingxBrokerOptions = bot.src.Brokers.Bingx.BrokerOptions;

namespace bot;

public class Program
{
    private static IConfigurationRoot Configuration { get; set; } = null!;
    private static ILogger Logger { get; set; } = null!;

    private static async Task Main(string[] args)
    {
        try
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(Configuration, new ConfigurationReaderOptions() { SectionName = ConfigurationKeys.SERILOG })
            .CreateLogger();

            BingxBrokerOptions brokerOptions = new();
            Configuration.Bind($"{ConfigurationKeys.BROKER_OPTIONS}:{Configuration[ConfigurationKeys.BROKER_NAME]}", brokerOptions);

            MessageStoreOptions messageStoreOptions = new();
            Configuration.Bind($"{ConfigurationKeys.MESSAGE_STORE_NAME}:{Configuration[ConfigurationKeys.MESSAGE_STORE_OPTIONS]}", messageStoreOptions);

            ICandleRepository candleRepository = new CandleRepository();

            IAccount account = BrokerFactory.CreateAccount(Configuration[ConfigurationKeys.BROKER_NAME]!, brokerOptions, Logger);
            ITrade trade = BrokerFactory.CreateTrade(Configuration[ConfigurationKeys.BROKER_NAME]!, brokerOptions, Logger);
            IBroker broker = BrokerFactory.CreateBroker(Configuration[ConfigurationKeys.BROKER_NAME]!, brokerOptions, trade, account, candleRepository, Logger);

            IMessageStore messageStore = MessageStoreFactory.CreateMessageStore(Configuration[ConfigurationKeys.MESSAGE_STORE_NAME]!, messageStoreOptions, Logger);
            Notifier notifier = new(Logger);

            ITime time = new Time();
            IStrategy strategy = StrategyFactory.CreateStrategy(Configuration[ConfigurationKeys.STRATEGY_NAME]!, messageStore, messageStoreOptions.SignalProviderEmail, Logger, time);

            await BotFactory.CreateBot(Configuration[ConfigurationKeys.BOT_NAME]!, broker, strategy, time, Logger).Run();
        }
        catch (System.Exception ex)
        {
            Logger.Error(ex, "An unhandled exception has been thrown.");
            try
            {
                Logger.Information(ex, "Notifying listeners...");
                await new Notifier(Logger).SendMessage($"FATAL: Unhandled exception: {ex.Message}");
                Logger.Information(ex, "Listeners are notified.");
            }
            catch (System.Exception)
            {
                Logger.Information(ex, "Failed to notify listeners.");
                throw;
            }
            return;
        }
    }
}
