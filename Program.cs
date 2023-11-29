using System.Text.Json;
using bingx_test;
using Google.Apis.Gmail.v1;

internal class Program
{
    private const string GMAIL_CLIENT_ID = "511671693440-o2fjs4lc2pfmttanjclbq7dccdeumemv.apps.googleusercontent.com";
    private const string GMAIL_CLIENT_SECRET = "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";
    private static readonly string[] GmailScopes = new string[] { GmailService.Scope.MailGoogleCom };

    // public const string SIGNAL_PROVIDER_EMAIL = "Saj.Aziznejad.jad@gmail.com";
    public const string SIGNAL_PROVIDER_EMAIL = "noreply@tradingview.com";

    private const string BINGX_API_KEY = "Mm6J3sTPRhOiuuSv07OXYGHnK0dLqTuZQ3kjK1UxebcVVjx0HbHfJnByOsNvFJMvKr9WbsrtCmPjgAwVjA";
    private const string BINGX_API_SECRET = "bKY5pmdTmQtGdCJpzUWtj9cmijpLGfaD3f8CKHLyKvCpehqqaLwYmEalfogm0br2n7JzWY9vrqWt7OoQIAg";
    private const string BASE_URI = "open-api-vst.bingx.com";
    private const string SYMBOL = "BTC-USDT";
    public const int TIME_FRAME = 5;
    public const float MARGIN = 200;
    public const int LEVERAGE = 50;
    public const float TP_PERCENTAGE = 3.8f;
    public const float SL_PERCENTAGE = 10f;

    public const string GMAIL = "taghalloby@gmail.com";

    private static void Main(string[] args)
    {
        // TradeOperations().Wait();
        // new Market(BASE_URI, BINGX_API_KEY, BINGX_API_SECRET, SYMBOL).GetLastPrice(SYMBOL, TIME_FRAME).Wait();
        // return;
        new GmailApiHelper(GMAIL_CLIENT_ID, GMAIL_CLIENT_SECRET, GmailScopes, SIGNAL_PROVIDER_EMAIL).DeleteAllEmails(GMAIL, SIGNAL_PROVIDER_EMAIL).Wait();
        new UTBot(
            new Trade(BASE_URI, BINGX_API_KEY, BINGX_API_SECRET, SYMBOL),
            new Market(BASE_URI, BINGX_API_KEY, BINGX_API_SECRET, SYMBOL),
            TIME_FRAME,
            new GmailApiHelper(GMAIL_CLIENT_ID, GMAIL_CLIENT_SECRET, GmailScopes, SIGNAL_PROVIDER_EMAIL),
            GMAIL,
            MARGIN,
            LEVERAGE,
            TP_PERCENTAGE,
            SL_PERCENTAGE
        ).Run().Wait();
    }

    private static async Task TradeOperations()
    {
        System.Console.WriteLine("\n\nTrade Operations begins...");

        bool isLong = false;
        System.Console.WriteLine("It's a " + (isLong ? "Long" : "Short") + " trade");

        float lastPrice = await new Market(BASE_URI, BINGX_API_KEY, BINGX_API_SECRET, SYMBOL).GetLastPrice(SYMBOL, TIME_FRAME);
        System.Console.WriteLine($"Last price is: {lastPrice}");

        // float quantity = 200f;
        float quantity = (float)(MARGIN * LEVERAGE / lastPrice);
        System.Console.WriteLine($"Quantity is: {quantity}");

        float sl = Trade.GetSl(isLong, 10, lastPrice, LEVERAGE);
        System.Console.WriteLine($"SL is: {sl}");

        float tp = Trade.GetTp(isLong, 10, lastPrice, LEVERAGE);
        System.Console.WriteLine($"TP is: {tp}");

        HttpResponseMessage response;

        System.Console.WriteLine("GET_BALANCE");
        response = await new Account(BASE_URI, BINGX_API_KEY, BINGX_API_SECRET, SYMBOL).GetBalance();
        await Utilities.HandleResponse(response);
        System.Console.WriteLine("");

        System.Console.WriteLine("GET_LEVERAGE");
        response = await new Trade(BASE_URI, BINGX_API_KEY, BINGX_API_SECRET, SYMBOL).GetLeverage();
        await Utilities.HandleResponse(response);
        System.Console.WriteLine("");

        System.Console.WriteLine("SET_LEVERAGE");
        response = await new Trade(BASE_URI, BINGX_API_KEY, BINGX_API_SECRET, SYMBOL).SetLeverage(LEVERAGE, true);
        await Utilities.HandleResponse(response);
        System.Console.WriteLine("");

        System.Console.WriteLine("SET_LEVERAGE");
        response = await new Trade(BASE_URI, BINGX_API_KEY, BINGX_API_SECRET, SYMBOL).SetLeverage(LEVERAGE, false);
        await Utilities.HandleResponse(response);
        System.Console.WriteLine("");

        System.Console.WriteLine("GET_LEVERAGE");
        response = await new Trade(BASE_URI, BINGX_API_KEY, BINGX_API_SECRET, SYMBOL).GetLeverage();
        await Utilities.HandleResponse(response);
        System.Console.WriteLine("");

        System.Console.WriteLine("DELETE_ORDERS");
        response = await new OrderManagement(BASE_URI, BINGX_API_KEY, BINGX_API_SECRET, SYMBOL).DeleteOrders();
        await Utilities.HandleResponse(response);
        System.Console.WriteLine("");

        System.Console.WriteLine("OPEN_MARKET_ORDER");
        response = await new Trade(BASE_URI, BINGX_API_KEY, BINGX_API_SECRET, SYMBOL).OpenMarketOrder(isLong, quantity, tp, sl);
        await Utilities.HandleResponse(response);
        System.Console.WriteLine("");

        // System.Console.WriteLine("OPEN_LIMIT_ORDER");
        // response = await new Trade(BASE_URI, BINGX_API_KEY, BINGX_API_SECRET, SYMBOL).OpenLimitOrder(isLong, quantity, lastPrice, tp, sl);
        // await Utilities.HandleResponse(response);
        // System.Console.WriteLine("");

        // System.Console.WriteLine("OPEN_TRIGGER_MARKET_ORDER");
        // response = await new Trade(BASE_URI, BINGX_API_KEY, BINGX_API_SECRET, SYMBOL).OpenTriggerMarketOrder(isLong, quantity, 0.07785f, tp, sl);
        // await Utilities.HandleResponse(response);
        // System.Console.WriteLine("");

        // System.Console.WriteLine("OPEN_TRIGGER_LIMIT_ORDER");
        // response = await new Trade(BASE_URI, BINGX_API_KEY, BINGX_API_SECRET, SYMBOL).OpenTriggerLimitOrder(isLong, quantity, lastPrice, 0.07546f, tp, sl);
        // await Utilities.HandleResponse(response);
        // System.Console.WriteLine("");

        System.Console.WriteLine("GET_ORDERS");
        response = await new OrderManagement(BASE_URI, BINGX_API_KEY, BINGX_API_SECRET, SYMBOL).GetOrders();
        await Utilities.HandleResponse(response);
        System.Console.WriteLine("");

        System.Console.WriteLine("\n\nTrade Operations finished...");
    }
}


// symbol = Symbol,
// type = "MARKET",
// side = "BUY",
// positionSide = isLong ? "LONG" : "SHORT",
// recvWindow = 10000,
// quoteOrderQty = 12,
// quantity,
// price = 40000
// stopPrice = 38000,
// stopLoss = JsonSerializer.Serialize(new
// {
//     type = "TAKE_PROFIT",
//     stopPrice = 38100,
//     workingType = "MARK_PRICE"
// }),
// takeProfit = JsonSerializer.Serialize(new
// {
//     type = "STOP",
//     stopPrice = 37900,
//     workingType = "MARK_PRICE"
// }),