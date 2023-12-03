using bingx_api;
using bingx_ut_bot;
using Microsoft.Extensions.Configuration;

internal class Program
{
    public static IConfigurationRoot Configuration { get; private set; } = null!;

    private static void Main(string[] args)
    {
        Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        // TradeOperations1().Wait();return;

        new Bot(Configuration.GetSection("BingxApi"), Configuration.GetSection("GmailApi")).Run().Wait();
    }

    private static async Task TradeOperations1()
    {
        System.Console.WriteLine("\n\nTrade Operations begins...");

        bool isLong = false;
        System.Console.WriteLine("It's a " + (isLong ? "Long" : "Short") + " trade");

        float lastPrice = await new Market(Configuration["BingxApi:BaseUri"]!, Configuration["BingxApi:ApiKey"]!, Configuration["BingxApi:ApiSecret"]!, Configuration["BingxApi:Symbol"]!).GetLastPrice(Configuration["BingxApi:Symbol"]!, int.Parse(Configuration["BingxApi:TimeFrame"]!));
        System.Console.WriteLine($"Last price is: {lastPrice}");

        float quantity = (float)(float.Parse(Configuration["BingxApi:Margin"]!) * int.Parse(Configuration["BingxApi:Leverage"]!) / lastPrice);
        System.Console.WriteLine($"Quantity is: {quantity}");

        float sl = Trade.CalculateSl(isLong, 10, lastPrice, int.Parse(Configuration["BingxApi:Leverage"]!));
        System.Console.WriteLine($"SL is: {sl}");

        float tp = Trade.CalculateTp(isLong, 10, lastPrice, int.Parse(Configuration["BingxApi:Leverage"]!));
        System.Console.WriteLine($"TP is: {tp}");

        HttpResponseMessage response;

        System.Console.WriteLine("GET_BALANCE");
        response = await new Account(Configuration["BingxApi:BaseUrl"]!, Configuration["BingxApi:ApiKey"]!, Configuration["BingxApi:ApiSecret"]!, Configuration["BingxApi:Symbol"]!).GetBalance();
        await Utilities.HandleBingxResponse(response);
        System.Console.WriteLine("");

        System.Console.WriteLine("GET_LEVERAGE");
        response = await new Trade(Configuration["BingxApi:BaseUrl"]!, Configuration["BingxApi:ApiKey"]!, Configuration["BingxApi:ApiSecret"]!, Configuration["BingxApi:Symbol"]!).GetLeverage();
        await Utilities.HandleBingxResponse(response);
        System.Console.WriteLine("");

        System.Console.WriteLine("SET_LEVERAGE");
        response = await new Trade(Configuration["BingxApi:BaseUrl"]!, Configuration["BingxApi:ApiKey"]!, Configuration["BingxApi:ApiSecret"]!, Configuration["BingxApi:Symbol"]!).SetLeverage(int.Parse(Configuration["BingxApi:Leverage"]!), true);
        await Utilities.HandleBingxResponse(response);
        System.Console.WriteLine("");

        System.Console.WriteLine("SET_LEVERAGE");
        response = await new Trade(Configuration["BingxApi:BaseUrl"]!, Configuration["BingxApi:ApiKey"]!, Configuration["BingxApi:ApiSecret"]!, Configuration["BingxApi:Symbol"]!).SetLeverage(int.Parse(Configuration["BingxApi:Leverage"]!), false);
        await Utilities.HandleBingxResponse(response);
        System.Console.WriteLine("");

        System.Console.WriteLine("GET_LEVERAGE");
        response = await new Trade(Configuration["BingxApi:BaseUrl"]!, Configuration["BingxApi:ApiKey"]!, Configuration["BingxApi:ApiSecret"]!, Configuration["BingxApi:Symbol"]!).GetLeverage();
        await Utilities.HandleBingxResponse(response);
        System.Console.WriteLine("");

        System.Console.WriteLine("DELETE_ORDERS");
        response = await new Trade(Configuration["BingxApi:BaseUrl"]!, Configuration["BingxApi:ApiKey"]!, Configuration["BingxApi:ApiSecret"]!, Configuration["BingxApi:Symbol"]!).CloseOrders();
        await Utilities.HandleBingxResponse(response);
        System.Console.WriteLine("");

        System.Console.WriteLine("CLOSE_OPEN_POSITIONS");
        response = await new Trade(Configuration["BingxApi:BaseUrl"]!, Configuration["BingxApi:ApiKey"]!, Configuration["BingxApi:ApiSecret"]!, Configuration["BingxApi:Symbol"]!).CloseOpenPositions();
        await Utilities.HandleBingxResponse(response);
        System.Console.WriteLine("");

        // System.Console.WriteLine("OPEN_MARKET_ORDER");
        // response = await new Trade(Configuration["BingxApi:BaseUrl"]!, Configuration["BingxApi:ApiKey"]!, Configuration["BingxApi:ApiSecret"]!, Configuration["BingxApi:Symbol"]!).OpenMarketOrder(isLong, quantity, tp, sl);
        // await Utilities.HandleBingxResponse(response);
        // System.Console.WriteLine("");

        // System.Console.WriteLine("OPEN_LIMIT_ORDER");
        // response = await new Trade(Configuration["BingxApi:BaseUrl"]!, Configuration["BingxApi:ApiKey"]!, Configuration["BingxApi:ApiSecret"]!, Configuration["BingxApi:Symbol"]!).OpenLimitOrder(isLong, quantity, lastPrice, tp, sl);
        // await Utilities.HandleResponse(response);
        // System.Console.WriteLine("");

        // System.Console.WriteLine("OPEN_TRIGGER_MARKET_ORDER");
        // response = await new Trade(Configuration["BingxApi:BaseUrl"]!, Configuration["BingxApi:ApiKey"]!, Configuration["BingxApi:ApiSecret"]!, Configuration["BingxApi:Symbol"]!).OpenTriggerMarketOrder(isLong, quantity, 0.07785f, tp, sl);
        // await Utilities.HandleResponse(response);
        // System.Console.WriteLine("");

        // System.Console.WriteLine("OPEN_TRIGGER_LIMIT_ORDER");
        // response = await new Trade(Configuration["BingxApi:BaseUrl"]!, Configuration["BingxApi:ApiKey"]!, Configuration["BingxApi:ApiSecret"]!, Configuration["BingxApi:Symbol"]!).OpenTriggerLimitOrder(isLong, quantity, lastPrice, 0.07546f, tp, sl);
        // await Utilities.HandleResponse(response);
        // System.Console.WriteLine("");

        System.Console.WriteLine("GET_ORDERS");
        response = await new Trade(Configuration["BingxApi:BaseUrl"]!, Configuration["BingxApi:ApiKey"]!, Configuration["BingxApi:ApiSecret"]!, Configuration["BingxApi:Symbol"]!).GetOrders();
        await Utilities.HandleBingxResponse(response);
        System.Console.WriteLine("");

        System.Console.WriteLine("\n\nTrade Operations finished...");
    }
}
