using broker_api.src;
using broker_api.src.Providers;
using email_api.Models;
using email_api.src;
using email_api.src.Providers;
using strategies.src;

namespace bot.src;

public class BotFactory
{
    public static IBot CreateBot() => Program.Configuration["StrategyName"] switch
    {
        "General" => CreateGeneralBot(),
        _ => throw new Exception($"Invalid configuration provided for StrategyName property. StrategyName: {Program.Configuration["StrategyName"]}")
    };

    public static IBot CreateGeneralBot() => new Bot(
            new GeneralStrategy(
                new GeneralSignals(
                    new GmailProvider(
                        new EmailProviderOptions()
                        {
                            OwnerGmail = Program.Configuration["GeneralStrategy:GmailApi:SignalProvider:Gmail"]!,
                            ClientId = Program.Configuration["GeneralStrategy:GmailApi:SignalProvider:ClientId"]!,
                            ClientSecret = Program.Configuration["GeneralStrategy:GmailApi:SignalProvider:ClientSecret"]!,
                            SignalProviderEmail = Program.Configuration["GeneralStrategy:GmailApi:SignalProvider:SignalProviderEmail"]!,
                            DataStoreFolderAddress = Program.Configuration["GeneralStrategy:GmailApi:SignalProvider:DataStoreFolderAddress"]!,
                        },
                        Program.Logger
                    ),
                    Program.Logger
                ),
                Program.Logger
            ),
            new Account(Program.Configuration["GeneralStrategy:BingxApi:BaseUrl"]!, Program.Configuration["GeneralStrategy:BingxApi:ApiKey"]!, Program.Configuration["GeneralStrategy:BingxApi:ApiSecret"]!, Program.Configuration["GeneralStrategy:BingxApi:Symbol"]!, new BingxUtilities(Program.Logger)),
            new Trade(Program.Configuration["GeneralStrategy:BingxApi:BaseUrl"]!, Program.Configuration["GeneralStrategy:BingxApi:ApiKey"]!, Program.Configuration["GeneralStrategy:BingxApi:ApiSecret"]!, Program.Configuration["GeneralStrategy:BingxApi:Symbol"]!, new BingxUtilities(Program.Logger)),
            new Market(Program.Configuration["GeneralStrategy:BingxApi:BaseUrl"]!, Program.Configuration["GeneralStrategy:BingxApi:ApiKey"]!, Program.Configuration["GeneralStrategy:BingxApi:ApiSecret"]!, Program.Configuration["GeneralStrategy:BingxApi:Symbol"]!, new BingxUtilities(Program.Logger)),
            int.Parse(Program.Configuration["GeneralStrategy:BingxApi:TimeFrame"]!),
            new BingxUtilities(Program.Logger),
            new Utilities()
        );
}
