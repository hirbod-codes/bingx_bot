using bot.src;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Settings.Configuration;

namespace bot;

public class Program
{
    public static IConfigurationRoot Configuration { get; private set; } = null!;
    public static Logger Logger { get; set; } = null!;

    private static void Main(string[] args)
    {
        try
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(Configuration, new ConfigurationReaderOptions() { SectionName = "Serilog" })
            .CreateLogger();

            BotFactory.CreateBot().Run().Wait();
        }
        finally { Logger?.Dispose(); }
    }
}
