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
        "UT" => CreateUTBot(),
        "SMMA" => CreateSMMABot(),
        "MACross" => CreateMACrossBot(),
        "Ichimoku" => CreateIchimokuBot(),
        _ => throw new Exception($"Invalid configuration provided for StrategyName property. StrategyName: {Program.Configuration["StrategyName"]}")
    };

    public static IBot CreateUTBot() => new Bot(
            new GeneralStrategy(
                new GeneralSignals(
                    new GmailProvider(
                        new EmailProviderOptions()
                        {
                            OwnerGmail = Program.Configuration["UT:GmailApi:SignalProvider:Gmail"]!,
                            ClientId = Program.Configuration["UT:GmailApi:SignalProvider:ClientId"]!,
                            ClientSecret = Program.Configuration["UT:GmailApi:SignalProvider:ClientSecret"]!,
                            SignalProviderEmail = Program.Configuration["UT:GmailApi:SignalProvider:SignalProviderEmail"]!,
                            DataStoreFolderAddress = Program.Configuration["UT:GmailApi:SignalProvider:DataStoreFolderAddress"]!,
                        },
                        Program.Logger
                    ),
                    Program.Logger
                ),
                Program.Logger
            ),
            new Account(Program.Configuration["UT:BingxApi:BaseUrl"]!, Program.Configuration["UT:BingxApi:ApiKey"]!, Program.Configuration["UT:BingxApi:ApiSecret"]!, Program.Configuration["UT:BingxApi:Symbol"]!, new BingxUtilities(Program.Logger)),
            new Trade(Program.Configuration["UT:BingxApi:BaseUrl"]!, Program.Configuration["UT:BingxApi:ApiKey"]!, Program.Configuration["UT:BingxApi:ApiSecret"]!, Program.Configuration["UT:BingxApi:Symbol"]!, new BingxUtilities(Program.Logger)),
            new Market(Program.Configuration["UT:BingxApi:BaseUrl"]!, Program.Configuration["UT:BingxApi:ApiKey"]!, Program.Configuration["UT:BingxApi:ApiSecret"]!, Program.Configuration["UT:BingxApi:Symbol"]!, new BingxUtilities(Program.Logger)),
            int.Parse(Program.Configuration["UT:BingxApi:TimeFrame"]!),
            new BingxUtilities(Program.Logger),
            new Utilities()
        );

    public static IBot CreateSMMABot() => new Bot(
            new GeneralStrategy(
                new GeneralSignals(
                    new GmailProvider(
                        new EmailProviderOptions()
                        {
                            OwnerGmail = Program.Configuration["SMMA:GmailApi:SignalProvider:Gmail"]!,
                            ClientId = Program.Configuration["SMMA:GmailApi:SignalProvider:ClientId"]!,
                            ClientSecret = Program.Configuration["SMMA:GmailApi:SignalProvider:ClientSecret"]!,
                            SignalProviderEmail = Program.Configuration["SMMA:GmailApi:SignalProvider:SignalProviderEmail"]!,
                            DataStoreFolderAddress = Program.Configuration["SMMA:GmailApi:SignalProvider:DataStoreFolderAddress"]!,
                        },
                        Program.Logger
                    ),
                    Program.Logger
                ),
                Program.Logger
            ),
            new Account(Program.Configuration["SMMA:BingxApi:BaseUrl"]!, Program.Configuration["SMMA:BingxApi:ApiKey"]!, Program.Configuration["SMMA:BingxApi:ApiSecret"]!, Program.Configuration["SMMA:BingxApi:Symbol"]!, new BingxUtilities(Program.Logger)),
            new Trade(Program.Configuration["SMMA:BingxApi:BaseUrl"]!, Program.Configuration["SMMA:BingxApi:ApiKey"]!, Program.Configuration["SMMA:BingxApi:ApiSecret"]!, Program.Configuration["SMMA:BingxApi:Symbol"]!, new BingxUtilities(Program.Logger)),
            new Market(Program.Configuration["SMMA:BingxApi:BaseUrl"]!, Program.Configuration["SMMA:BingxApi:ApiKey"]!, Program.Configuration["SMMA:BingxApi:ApiSecret"]!, Program.Configuration["SMMA:BingxApi:Symbol"]!, new BingxUtilities(Program.Logger)),
            int.Parse(Program.Configuration["SMMA:BingxApi:TimeFrame"]!),
            new BingxUtilities(Program.Logger),
            new Utilities()
        );

    public static IBot CreateMACrossBot() => new Bot(
            new GeneralStrategy(
                new GeneralSignals(
                    new GmailProvider(
                        new EmailProviderOptions()
                        {
                            OwnerGmail = Program.Configuration["MACross:GmailApi:SignalProvider:Gmail"]!,
                            ClientId = Program.Configuration["MACross:GmailApi:SignalProvider:ClientId"]!,
                            ClientSecret = Program.Configuration["MACross:GmailApi:SignalProvider:ClientSecret"]!,
                            SignalProviderEmail = Program.Configuration["MACross:GmailApi:SignalProvider:SignalProviderEmail"]!,
                            DataStoreFolderAddress = Program.Configuration["MACross:GmailApi:SignalProvider:DataStoreFolderAddress"]!,
                        },
                        Program.Logger
                    ),
                    Program.Logger
                ),
                Program.Logger
            ),
            new Account(Program.Configuration["MACross:BingxApi:BaseUrl"]!, Program.Configuration["MACross:BingxApi:ApiKey"]!, Program.Configuration["MACross:BingxApi:ApiSecret"]!, Program.Configuration["MACross:BingxApi:Symbol"]!, new BingxUtilities(Program.Logger)),
            new Trade(Program.Configuration["MACross:BingxApi:BaseUrl"]!, Program.Configuration["MACross:BingxApi:ApiKey"]!, Program.Configuration["MACross:BingxApi:ApiSecret"]!, Program.Configuration["MACross:BingxApi:Symbol"]!, new BingxUtilities(Program.Logger)),
            new Market(Program.Configuration["MACross:BingxApi:BaseUrl"]!, Program.Configuration["MACross:BingxApi:ApiKey"]!, Program.Configuration["MACross:BingxApi:ApiSecret"]!, Program.Configuration["MACross:BingxApi:Symbol"]!, new BingxUtilities(Program.Logger)),
            int.Parse(Program.Configuration["MACross:BingxApi:TimeFrame"]!),
            new BingxUtilities(Program.Logger),
            new Utilities()
        );

    public static IBot CreateIchimokuBot() => new Bot(
            new GeneralStrategy(
                new GeneralSignals(
                    new GmailProvider(
                        new EmailProviderOptions()
                        {
                            OwnerGmail = Program.Configuration["Ichimoku:GmailApi:SignalProvider:Gmail"]!,
                            ClientId = Program.Configuration["Ichimoku:GmailApi:SignalProvider:ClientId"]!,
                            ClientSecret = Program.Configuration["Ichimoku:GmailApi:SignalProvider:ClientSecret"]!,
                            SignalProviderEmail = Program.Configuration["Ichimoku:GmailApi:SignalProvider:SignalProviderEmail"]!,
                            DataStoreFolderAddress = Program.Configuration["Ichimoku:GmailApi:SignalProvider:DataStoreFolderAddress"]!,
                        },
                        Program.Logger
                    ),
                    Program.Logger
                ),
                Program.Logger
            ),
            new Account(Program.Configuration["Ichimoku:BingxApi:BaseUrl"]!, Program.Configuration["Ichimoku:BingxApi:ApiKey"]!, Program.Configuration["Ichimoku:BingxApi:ApiSecret"]!, Program.Configuration["Ichimoku:BingxApi:Symbol"]!, new BingxUtilities(Program.Logger)),
            new Trade(Program.Configuration["Ichimoku:BingxApi:BaseUrl"]!, Program.Configuration["Ichimoku:BingxApi:ApiKey"]!, Program.Configuration["Ichimoku:BingxApi:ApiSecret"]!, Program.Configuration["Ichimoku:BingxApi:Symbol"]!, new BingxUtilities(Program.Logger)),
            new Market(Program.Configuration["Ichimoku:BingxApi:BaseUrl"]!, Program.Configuration["Ichimoku:BingxApi:ApiKey"]!, Program.Configuration["Ichimoku:BingxApi:ApiSecret"]!, Program.Configuration["Ichimoku:BingxApi:Symbol"]!, new BingxUtilities(Program.Logger)),
            int.Parse(Program.Configuration["Ichimoku:BingxApi:TimeFrame"]!),
            new BingxUtilities(Program.Logger),
            new Utilities()
        );
}
