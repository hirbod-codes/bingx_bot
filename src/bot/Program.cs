using bingx_api;
using bot.src;
using Microsoft.Extensions.Configuration;
using Serilog.Core;

namespace bot;

public class Program
{
    public static IConfigurationRoot Configuration { get; private set; } = null!;
    public static Logger Logger { get; set; } = null!;

    private static void Main(string[] args)
    {
        try
        {
            Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            System.Console.WriteLine(Configuration["UTStrategy:Serilog:WriteTo:1:Args:serverUrl"]);

            Configuration["UTStrategy:GmailApi:LongProvider:SignalProviderEmail"] = "ema.cross.up@gmail.com";
            Configuration["UTStrategy:GmailApi:ShortProvider:SignalProviderEmail"] = "ema.cross.up@gmail.com";

            BotFactory.CreateBot().Run().Wait();
        }
        finally { Logger?.Dispose(); }
    }

    private static async Task TradeOperations1()
    {
        System.Console.WriteLine("\n\nTrade Operations begins...");

        Utilities utilities = new(Logger);

        bool isLong = false;
        System.Console.WriteLine("It's a " + (isLong ? "Long" : "Short") + " trade");

        string strategyName = Configuration["StrategyName"]!;

        float lastPrice = await new Market(Configuration[$"{strategyName}:BingxApi:BaseUri"]!, Configuration[$"{strategyName}:BingxApi:ApiKey"]!, Configuration[$"{strategyName}:BingxApi:ApiSecret"]!, Configuration[$"{strategyName}:BingxApi:Symbol"]!, utilities).GetLastPrice(Configuration[$"{strategyName}:BingxApi:Symbol"]!, int.Parse(Configuration[$"{strategyName}:BingxApi:TimeFrame"]!));
        System.Console.WriteLine($"Last price is: {lastPrice}");

        float quantity = (float)(float.Parse(Configuration[$"{strategyName}:BingxApi:Margin"]!) * int.Parse(Configuration[$"{strategyName}:BingxApi:Leverage"]!) / lastPrice);
        System.Console.WriteLine($"Quantity is: {quantity}");

        float sl = Trade.CalculateSl(isLong, 10, lastPrice, int.Parse(Configuration[$"{strategyName}:BingxApi:Leverage"]!));
        System.Console.WriteLine($"SL is: {sl}");

        float tp = Trade.CalculateTp(isLong, 10, lastPrice, int.Parse(Configuration[$"{strategyName}:BingxApi:Leverage"]!));
        System.Console.WriteLine($"TP is: {tp}");

        HttpResponseMessage response;

        System.Console.WriteLine("GET_BALANCE");
        response = await new Account(Configuration[$"{strategyName}:BingxApi:BaseUrl"]!, Configuration[$"{strategyName}:BingxApi:ApiKey"]!, Configuration[$"{strategyName}:BingxApi:ApiSecret"]!, Configuration[$"{strategyName}:BingxApi:Symbol"]!, utilities).GetBalance();
        await utilities.HandleBingxResponse(response);
        System.Console.WriteLine("");

        // System.Console.WriteLine("GET_LEVERAGE");
        // response = await new Trade(Configuration[$"{strategyName}:BingxApi:BaseUrl"]!, Configuration[$"{strategyName}:BingxApi:ApiKey"]!, Configuration[$"{strategyName}:BingxApi:ApiSecret"]!, Configuration[$"{strategyName}:BingxApi:Symbol"]!).GetLeverage();
        // await utilities.HandleBingxResponse(response);
        // System.Console.WriteLine("");

        // System.Console.WriteLine("SET_LEVERAGE");
        // response = await new Trade(Configuration[$"{strategyName}:BingxApi:BaseUrl"]!, Configuration[$"{strategyName}:BingxApi:ApiKey"]!, Configuration[$"{strategyName}:BingxApi:ApiSecret"]!, Configuration[$"{strategyName}:BingxApi:Symbol"]!).SetLeverage(int.Parse(Configuration[$"{strategyName}:BingxApi:Leverage"]!), true);
        // await utilities.HandleBingxResponse(response);
        // System.Console.WriteLine("");

        // System.Console.WriteLine("SET_LEVERAGE");
        // response = await new Trade(Configuration[$"{strategyName}:BingxApi:BaseUrl"]!, Configuration[$"{strategyName}:BingxApi:ApiKey"]!, Configuration[$"{strategyName}:BingxApi:ApiSecret"]!, Configuration[$"{strategyName}:BingxApi:Symbol"]!).SetLeverage(int.Parse(Configuration[$"{strategyName}:BingxApi:Leverage"]!), false);
        // await utilities.HandleBingxResponse(response);
        // System.Console.WriteLine("");

        // System.Console.WriteLine("GET_LEVERAGE");
        // response = await new Trade(Configuration[$"{strategyName}:BingxApi:BaseUrl"]!, Configuration[$"{strategyName}:BingxApi:ApiKey"]!, Configuration[$"{strategyName}:BingxApi:ApiSecret"]!, Configuration[$"{strategyName}:BingxApi:Symbol"]!).GetLeverage();
        // await utilities.HandleBingxResponse(response);
        // System.Console.WriteLine("");

        // System.Console.WriteLine("DELETE_ORDERS");
        // response = await new Trade(Configuration[$"{strategyName}:BingxApi:BaseUrl"]!, Configuration[$"{strategyName}:BingxApi:ApiKey"]!, Configuration[$"{strategyName}:BingxApi:ApiSecret"]!, Configuration[$"{strategyName}:BingxApi:Symbol"]!).CloseOrders();
        // await utilities.HandleBingxResponse(response);
        // System.Console.WriteLine("");

        // System.Console.WriteLine("CLOSE_OPEN_POSITIONS");
        // response = await new Trade(Configuration[$"{strategyName}:BingxApi:BaseUrl"]!, Configuration[$"{strategyName}:BingxApi:ApiKey"]!, Configuration[$"{strategyName}:BingxApi:ApiSecret"]!, Configuration[$"{strategyName}:BingxApi:Symbol"]!).CloseOpenPositions();
        // await utilities.HandleBingxResponse(response);
        // System.Console.WriteLine("");

        // System.Console.WriteLine("OPEN_MARKET_ORDER");
        // response = await new Trade(Configuration[$"{strategyName}:BingxApi:BaseUrl"]!, Configuration[$"{strategyName}:BingxApi:ApiKey"]!, Configuration[$"{strategyName}:BingxApi:ApiSecret"]!, Configuration[$"{strategyName}:BingxApi:Symbol"]!).OpenMarketOrder(isLong, quantity, tp, sl);
        // await utilities.HandleBingxResponse(response);
        // System.Console.WriteLine("");

        // System.Console.WriteLine("OPEN_LIMIT_ORDER");
        // response = await new Trade(Configuration[$"{strategyName}:BingxApi:BaseUrl"]!, Configuration[$"{strategyName}:BingxApi:ApiKey"]!, Configuration[$"{strategyName}:BingxApi:ApiSecret"]!, Configuration[$"{strategyName}:BingxApi:Symbol"]!).OpenLimitOrder(isLong, quantity, lastPrice, tp, sl);
        // await utilities.HandleResponse(response);
        // System.Console.WriteLine("");

        // System.Console.WriteLine("OPEN_TRIGGER_MARKET_ORDER");
        // response = await new Trade(Configuration[$"{strategyName}:BingxApi:BaseUrl"]!, Configuration[$"{strategyName}:BingxApi:ApiKey"]!, Configuration[$"{strategyName}:BingxApi:ApiSecret"]!, Configuration[$"{strategyName}:BingxApi:Symbol"]!).OpenTriggerMarketOrder(isLong, quantity, 0.07785f, tp, sl);
        // await utilities.HandleResponse(response);
        // System.Console.WriteLine("");

        // System.Console.WriteLine("OPEN_TRIGGER_LIMIT_ORDER");
        // response = await new Trade(Configuration[$"{strategyName}:BingxApi:BaseUrl"]!, Configuration[$"{strategyName}:BingxApi:ApiKey"]!, Configuration[$"{strategyName}:BingxApi:ApiSecret"]!, Configuration[$"{strategyName}:BingxApi:Symbol"]!).OpenTriggerLimitOrder(isLong, quantity, lastPrice, 0.07546f, tp, sl);
        // await utilities.HandleResponse(response);
        // System.Console.WriteLine("");

        System.Console.WriteLine("GET_ORDERS");
        response = await new Trade(Configuration[$"{strategyName}:BingxApi:BaseUrl"]!, Configuration[$"{strategyName}:BingxApi:ApiKey"]!, Configuration[$"{strategyName}:BingxApi:ApiSecret"]!, Configuration[$"{strategyName}:BingxApi:Symbol"]!, utilities).GetOrders(new DateTime(2023, 12, 1, 22, 15, 0, DateTimeKind.Utc), DateTime.UtcNow);
        await utilities.HandleBingxResponse(response);
        System.Console.WriteLine("");

        // System.Console.WriteLine("GET_ORDERS");
        // response = await new Trade(Configuration[$"{strategyName}:BingxApi:BaseUrl"]!, Configuration[$"{strategyName}:BingxApi:ApiKey"]!, Configuration[$"{strategyName}:BingxApi:ApiSecret"]!, Configuration[$"{strategyName}:BingxApi:Symbol"]!).GetOrders();
        // await utilities.HandleBingxResponse(response);
        // System.Console.WriteLine("");

        System.Console.WriteLine("\n\nTrade Operations finished...");
    }
}
