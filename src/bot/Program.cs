using bot.src;
using Microsoft.Extensions.Configuration;

namespace bot;

public class Program
{
    public static IConfigurationRoot Configuration { get; private set; } = null!;

    private static void Main(string[] args)
    {
        Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        BotFactory.CreateUTBot(Configuration).Run().Wait();
    }
}
