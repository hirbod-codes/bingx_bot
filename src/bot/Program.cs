using bot.src.Bots;
using bot.src.Brokers;
using bot.src.MessageStores;
using bot.src.Notifiers.NTFY;
using bot.src.Strategies;
using bot.src.Util;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Settings.Configuration;

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
            .ReadFrom.Configuration(Configuration, new ConfigurationReaderOptions() { SectionName = "Serilog" })
            .CreateLogger();

            await new BotFactory(Configuration, Logger, new StrategyFactory(Configuration, Logger, new MessageStoreFactory(Configuration, Logger)), new BrokerFactory(Configuration, Logger), new Time())
            .CreateBot()
            .Run();
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
