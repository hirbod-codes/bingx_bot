using bingx_test;
using Microsoft.Extensions.Configuration;

internal class Program
{
    public static IConfigurationRoot Configuration { get; private set; } = null!;

    private static void Main(string[] args)
    {
        Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        new UTBot(Configuration.GetSection("BingxApi"), Configuration.GetSection("GmailApi")).Run().Wait();
    }
}
