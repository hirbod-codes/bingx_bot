using System.Text.Json;
using System.Text.Json.Nodes;
using bingx_api;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace bot;

public class Program
{
    public static IConfigurationRoot Configuration { get; private set; } = null!;

    private static void Main(string[] args)
    {
        Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        TradeOperations1("UTStrategy").Wait();
        // BotFactory.CreateEMABot(Configuration).Run().Wait();
        // BotFactory.CreateUTBot(Configuration).Run().Wait();
    }

    private static async Task TradeOperations1(string strategyName)
    {
        System.Console.WriteLine("\n\nTrade Operations begins...");

        bool isLong = false;
        System.Console.WriteLine("It's a " + (isLong ? "Long" : "Short") + " trade");

        float lastPrice = await new Market(Configuration[$"{strategyName}:BingxApi:BaseUri"]!, Configuration[$"{strategyName}:BingxApi:ApiKey"]!, Configuration[$"{strategyName}:BingxApi:ApiSecret"]!, Configuration[$"{strategyName}:BingxApi:Symbol"]!).GetLastPrice(Configuration[$"{strategyName}:BingxApi:Symbol"]!, int.Parse(Configuration[$"{strategyName}:BingxApi:TimeFrame"]!));
        System.Console.WriteLine($"Last price is: {lastPrice}");

        float quantity = (float)(float.Parse(Configuration[$"{strategyName}:BingxApi:Margin"]!) * int.Parse(Configuration[$"{strategyName}:BingxApi:Leverage"]!) / lastPrice);
        System.Console.WriteLine($"Quantity is: {quantity}");

        float sl = Trade.CalculateSl(isLong, 10, lastPrice, int.Parse(Configuration[$"{strategyName}:BingxApi:Leverage"]!));
        System.Console.WriteLine($"SL is: {sl}");

        float tp = Trade.CalculateTp(isLong, 10, lastPrice, int.Parse(Configuration[$"{strategyName}:BingxApi:Leverage"]!));
        System.Console.WriteLine($"TP is: {tp}");

        HttpResponseMessage response;

        System.Console.WriteLine("GET_BALANCE");
        response = await new Account(Configuration[$"{strategyName}:BingxApi:BaseUrl"]!, Configuration[$"{strategyName}:BingxApi:ApiKey"]!, Configuration[$"{strategyName}:BingxApi:ApiSecret"]!, Configuration[$"{strategyName}:BingxApi:Symbol"]!).GetBalance();
        await Utilities.HandleBingxResponse(response);
        System.Console.WriteLine("");

        // System.Console.WriteLine("GET_LEVERAGE");
        // response = await new Trade(Configuration[$"{strategyName}:BingxApi:BaseUrl"]!, Configuration[$"{strategyName}:BingxApi:ApiKey"]!, Configuration[$"{strategyName}:BingxApi:ApiSecret"]!, Configuration[$"{strategyName}:BingxApi:Symbol"]!).GetLeverage();
        // await Utilities.HandleBingxResponse(response);
        // System.Console.WriteLine("");

        // System.Console.WriteLine("SET_LEVERAGE");
        // response = await new Trade(Configuration[$"{strategyName}:BingxApi:BaseUrl"]!, Configuration[$"{strategyName}:BingxApi:ApiKey"]!, Configuration[$"{strategyName}:BingxApi:ApiSecret"]!, Configuration[$"{strategyName}:BingxApi:Symbol"]!).SetLeverage(int.Parse(Configuration[$"{strategyName}:BingxApi:Leverage"]!), true);
        // await Utilities.HandleBingxResponse(response);
        // System.Console.WriteLine("");

        // System.Console.WriteLine("SET_LEVERAGE");
        // response = await new Trade(Configuration[$"{strategyName}:BingxApi:BaseUrl"]!, Configuration[$"{strategyName}:BingxApi:ApiKey"]!, Configuration[$"{strategyName}:BingxApi:ApiSecret"]!, Configuration[$"{strategyName}:BingxApi:Symbol"]!).SetLeverage(int.Parse(Configuration[$"{strategyName}:BingxApi:Leverage"]!), false);
        // await Utilities.HandleBingxResponse(response);
        // System.Console.WriteLine("");

        // System.Console.WriteLine("GET_LEVERAGE");
        // response = await new Trade(Configuration[$"{strategyName}:BingxApi:BaseUrl"]!, Configuration[$"{strategyName}:BingxApi:ApiKey"]!, Configuration[$"{strategyName}:BingxApi:ApiSecret"]!, Configuration[$"{strategyName}:BingxApi:Symbol"]!).GetLeverage();
        // await Utilities.HandleBingxResponse(response);
        // System.Console.WriteLine("");

        // System.Console.WriteLine("DELETE_ORDERS");
        // response = await new Trade(Configuration[$"{strategyName}:BingxApi:BaseUrl"]!, Configuration[$"{strategyName}:BingxApi:ApiKey"]!, Configuration[$"{strategyName}:BingxApi:ApiSecret"]!, Configuration[$"{strategyName}:BingxApi:Symbol"]!).CloseOrders();
        // await Utilities.HandleBingxResponse(response);
        // System.Console.WriteLine("");

        // System.Console.WriteLine("CLOSE_OPEN_POSITIONS");
        // response = await new Trade(Configuration[$"{strategyName}:BingxApi:BaseUrl"]!, Configuration[$"{strategyName}:BingxApi:ApiKey"]!, Configuration[$"{strategyName}:BingxApi:ApiSecret"]!, Configuration[$"{strategyName}:BingxApi:Symbol"]!).CloseOpenPositions();
        // await Utilities.HandleBingxResponse(response);
        // System.Console.WriteLine("");

        // System.Console.WriteLine("OPEN_MARKET_ORDER");
        // response = await new Trade(Configuration[$"{strategyName}:BingxApi:BaseUrl"]!, Configuration[$"{strategyName}:BingxApi:ApiKey"]!, Configuration[$"{strategyName}:BingxApi:ApiSecret"]!, Configuration[$"{strategyName}:BingxApi:Symbol"]!).OpenMarketOrder(isLong, quantity, tp, sl);
        // await Utilities.HandleBingxResponse(response);
        // System.Console.WriteLine("");

        // System.Console.WriteLine("OPEN_LIMIT_ORDER");
        // response = await new Trade(Configuration[$"{strategyName}:BingxApi:BaseUrl"]!, Configuration[$"{strategyName}:BingxApi:ApiKey"]!, Configuration[$"{strategyName}:BingxApi:ApiSecret"]!, Configuration[$"{strategyName}:BingxApi:Symbol"]!).OpenLimitOrder(isLong, quantity, lastPrice, tp, sl);
        // await Utilities.HandleResponse(response);
        // System.Console.WriteLine("");

        // System.Console.WriteLine("OPEN_TRIGGER_MARKET_ORDER");
        // response = await new Trade(Configuration[$"{strategyName}:BingxApi:BaseUrl"]!, Configuration[$"{strategyName}:BingxApi:ApiKey"]!, Configuration[$"{strategyName}:BingxApi:ApiSecret"]!, Configuration[$"{strategyName}:BingxApi:Symbol"]!).OpenTriggerMarketOrder(isLong, quantity, 0.07785f, tp, sl);
        // await Utilities.HandleResponse(response);
        // System.Console.WriteLine("");

        // System.Console.WriteLine("OPEN_TRIGGER_LIMIT_ORDER");
        // response = await new Trade(Configuration[$"{strategyName}:BingxApi:BaseUrl"]!, Configuration[$"{strategyName}:BingxApi:ApiKey"]!, Configuration[$"{strategyName}:BingxApi:ApiSecret"]!, Configuration[$"{strategyName}:BingxApi:Symbol"]!).OpenTriggerLimitOrder(isLong, quantity, lastPrice, 0.07546f, tp, sl);
        // await Utilities.HandleResponse(response);
        // System.Console.WriteLine("");

        System.Console.WriteLine("GET_ORDERS");
        response = await new Trade(Configuration[$"{strategyName}:BingxApi:BaseUrl"]!, Configuration[$"{strategyName}:BingxApi:ApiKey"]!, Configuration[$"{strategyName}:BingxApi:ApiSecret"]!, Configuration[$"{strategyName}:BingxApi:Symbol"]!).GetOrders(new DateTime(2023, 12, 1, 22, 15, 0, DateTimeKind.Utc), DateTime.UtcNow);
        await Utilities.HandleBingxResponse(response);
        System.Console.WriteLine("");

        // System.Console.WriteLine("GET_ORDERS");
        // response = await new Trade(Configuration[$"{strategyName}:BingxApi:BaseUrl"]!, Configuration[$"{strategyName}:BingxApi:ApiKey"]!, Configuration[$"{strategyName}:BingxApi:ApiSecret"]!, Configuration[$"{strategyName}:BingxApi:Symbol"]!).GetOrders();
        // await Utilities.HandleBingxResponse(response);
        // System.Console.WriteLine("");

        System.Console.WriteLine("\n\nTrade Operations finished...");
    }
}
