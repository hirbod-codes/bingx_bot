using gmail_api.Models;
using Microsoft.Extensions.Configuration;
using strategies.src.EMA;
using strategies.src.UT;

namespace bot.src;

public class BotFactory
{
    public static IBot CreateEMABot(IConfigurationRoot configuration) => new Bot(
            configuration.GetSection("EMAStrategy:BingxApi"),
            new EMAStrategy(new EMASignals(
                new GmailProvider()
                {
                    OwnerGmail = configuration["EMAStrategy:GmailApi:LongProvider:Gmail"]!,
                    ClientId = configuration["EMAStrategy:GmailApi:LongProvider:ClientId"]!,
                    ClientSecret = configuration["EMAStrategy:GmailApi:LongProvider:ClientSecret"]!,
                    SignalProviderEmail = configuration["EMAStrategy:GmailApi:LongProvider:SignalProviderEmail"]!,
                    DataStoreFolderAddress = configuration["EMAStrategy:GmailApi:LongProvider:DataStoreFolderAddress"]!,
                },
                new GmailProvider()
                {
                    OwnerGmail = configuration["EMAStrategy:GmailApi:ShortProvider:Gmail"]!,
                    ClientId = configuration["EMAStrategy:GmailApi:ShortProvider:ClientId"]!,
                    ClientSecret = configuration["EMAStrategy:GmailApi:ShortProvider:ClientSecret"]!,
                    SignalProviderEmail = configuration["EMAStrategy:GmailApi:ShortProvider:SignalProviderEmail"]!,
                    DataStoreFolderAddress = configuration["EMAStrategy:GmailApi:ShortProvider:DataStoreFolderAddress"]!,
                }
            ))
        );

    public static IBot CreateUTBot(IConfigurationRoot configuration) => new Bot(
            configuration.GetSection("UTStrategy:BingxApi"),
            new UTStrategy(new UTSignals(
                new GmailProvider()
                {
                    OwnerGmail = configuration["UTStrategy:GmailApi:LongProvider:Gmail"]!,
                    ClientId = configuration["UTStrategy:GmailApi:LongProvider:ClientId"]!,
                    ClientSecret = configuration["UTStrategy:GmailApi:LongProvider:ClientSecret"]!,
                    SignalProviderEmail = configuration["UTStrategy:GmailApi:LongProvider:SignalProviderEmail"]!,
                    DataStoreFolderAddress = configuration["UTStrategy:GmailApi:LongProvider:DataStoreFolderAddress"]!,
                },
                new GmailProvider()
                {
                    OwnerGmail = configuration["UTStrategy:GmailApi:ShortProvider:Gmail"]!,
                    ClientId = configuration["UTStrategy:GmailApi:ShortProvider:ClientId"]!,
                    ClientSecret = configuration["UTStrategy:GmailApi:ShortProvider:ClientSecret"]!,
                    SignalProviderEmail = configuration["UTStrategy:GmailApi:ShortProvider:SignalProviderEmail"]!,
                    DataStoreFolderAddress = configuration["UTStrategy:GmailApi:ShortProvider:DataStoreFolderAddress"]!,
                }
            ))
        );
}
