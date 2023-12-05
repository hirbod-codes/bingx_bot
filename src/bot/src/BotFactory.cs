using gmail_api.Models;
using Serilog;
using Serilog.Settings.Configuration;
using strategies.src.EMA;
using strategies.src.UT;

namespace bot.src;

public class BotFactory
{
    public static IBot CreateEMABot()
    {
        Program.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(Program.Configuration, new ConfigurationReaderOptions() { SectionName = "EMAStrategy:Serilog" })
        .CreateLogger();

        return new Bot(
            Program.Configuration.GetSection("EMAStrategy:BingxApi"),
            new EMAStrategy(new EMASignals(
                new GmailProvider()
                {
                    OwnerGmail = Program.Configuration["EMAStrategy:GmailApi:LongProvider:Gmail"]!,
                    ClientId = Program.Configuration["EMAStrategy:GmailApi:LongProvider:ClientId"]!,
                    ClientSecret = Program.Configuration["EMAStrategy:GmailApi:LongProvider:ClientSecret"]!,
                    SignalProviderEmail = Program.Configuration["EMAStrategy:GmailApi:LongProvider:SignalProviderEmail"]!,
                    DataStoreFolderAddress = Program.Configuration["EMAStrategy:GmailApi:LongProvider:DataStoreFolderAddress"]!,
                },
                new GmailProvider()
                {
                    OwnerGmail = Program.Configuration["EMAStrategy:GmailApi:ShortProvider:Gmail"]!,
                    ClientId = Program.Configuration["EMAStrategy:GmailApi:ShortProvider:ClientId"]!,
                    ClientSecret = Program.Configuration["EMAStrategy:GmailApi:ShortProvider:ClientSecret"]!,
                    SignalProviderEmail = Program.Configuration["EMAStrategy:GmailApi:ShortProvider:SignalProviderEmail"]!,
                    DataStoreFolderAddress = Program.Configuration["EMAStrategy:GmailApi:ShortProvider:DataStoreFolderAddress"]!,
                }
            ))
        );
    }

    public static IBot CreateUTBot()
    {
        Program.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(Program.Configuration, new ConfigurationReaderOptions() { SectionName = "UTStrategy:Serilog" })
        .CreateLogger();

        return new Bot(
            Program.Configuration.GetSection("UTStrategy:BingxApi"),
            new UTStrategy(new UTSignals(
                new GmailProvider()
                {
                    OwnerGmail = Program.Configuration["UTStrategy:GmailApi:LongProvider:Gmail"]!,
                    ClientId = Program.Configuration["UTStrategy:GmailApi:LongProvider:ClientId"]!,
                    ClientSecret = Program.Configuration["UTStrategy:GmailApi:LongProvider:ClientSecret"]!,
                    SignalProviderEmail = Program.Configuration["UTStrategy:GmailApi:LongProvider:SignalProviderEmail"]!,
                    DataStoreFolderAddress = Program.Configuration["UTStrategy:GmailApi:LongProvider:DataStoreFolderAddress"]!,
                },
                new GmailProvider()
                {
                    OwnerGmail = Program.Configuration["UTStrategy:GmailApi:ShortProvider:Gmail"]!,
                    ClientId = Program.Configuration["UTStrategy:GmailApi:ShortProvider:ClientId"]!,
                    ClientSecret = Program.Configuration["UTStrategy:GmailApi:ShortProvider:ClientSecret"]!,
                    SignalProviderEmail = Program.Configuration["UTStrategy:GmailApi:ShortProvider:SignalProviderEmail"]!,
                    DataStoreFolderAddress = Program.Configuration["UTStrategy:GmailApi:ShortProvider:DataStoreFolderAddress"]!,
                }
            ))
        );
    }

    public static IBot CreateBot() => Program.Configuration["StrategyName"] switch
    {
        "UT" => CreateUTBot(),
        "EMA" => CreateEMABot(),
        _ => throw new Exception($"Invalid configuration provided for StrategyName property. StrategyName: {Program.Configuration["StrategyName"]}")
    };
}
