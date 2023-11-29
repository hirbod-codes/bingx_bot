using System.Text.Json;

namespace bingx_test;

public class Trade : Api
{
    public Trade(string base_url, string apiKey, string apiSecret, string symbol) : base(base_url, apiKey, apiSecret, symbol) { }

    public async Task<HttpResponseMessage> GetLeverage() => await Utilities.HandleRequest("https", Base_Url, "/openApi/swap/v2/trade/leverage", "GET", ApiKey, ApiSecret, new
    {
        symbol = Symbol
    });

    public async Task<HttpResponseMessage> SetLeverage(int leverage, bool isLong) => await Utilities.HandleRequest("https", Base_Url, "/openApi/swap/v2/trade/leverage", "POST", ApiKey, ApiSecret, new
    {
        symbol = Symbol,
        side = isLong ? "LONG" : "SHORT",
        leverage
    });

    public async Task<HttpResponseMessage> OpenMarketOrder(bool isLong, float quantity, float tp, float sl) => await Utilities.HandleRequest("https", Base_Url, "/openApi/swap/v2/trade/order", "POST", ApiKey, ApiSecret, new
    {
        symbol = Symbol,
        type = "MARKET",
        side = isLong ? "BUY" : "SELL",
        positionSide = isLong ? "LONG" : "SHORT",
        quantity,
        // recvWindow = 1000,
        takeProfit = JsonSerializer.Serialize(new
        {
            type = "TAKE_PROFIT_MARKET",
            quantity,
            stopPrice = tp,
            price = tp,
            workingType = "MARK_PRICE"
        }),
        stopLoss = JsonSerializer.Serialize(new
        {
            type = "STOP_MARKET",
            quantity,
            stopPrice = sl,
            price = sl,
            workingType = "MARK_PRICE"
        })
    });

    public async Task<HttpResponseMessage> OpenLimitOrder(bool isLong, float quantity, float price, float tp, float sl) => await Utilities.HandleRequest("https", Base_Url, "/openApi/swap/v2/trade/order", "POST", ApiKey, ApiSecret, new
    {
        symbol = Symbol,
        type = "LIMIT",
        side = isLong ? "BUY" : "SELL",
        positionSide = isLong ? "LONG" : "SHORT",
        price,
        quantity,
        // recvWindow = 1000,
        takeProfit = JsonSerializer.Serialize(new
        {
            type = "TAKE_PROFIT_MARKET",
            quantity,
            stopPrice = tp,
            price = tp,
            workingType = "MARK_PRICE"
        }),
        stopLoss = JsonSerializer.Serialize(new
        {
            type = "STOP_MARKET",
            quantity,
            stopPrice = sl,
            price = sl,
            workingType = "MARK_PRICE"
        })
    });

    public async Task<HttpResponseMessage> OpenTriggerLimitOrder(bool isLong, float quantity, float price, float stopPrice, float tp, float sl) => await Utilities.HandleRequest("https", Base_Url, "/openApi/swap/v2/trade/order", "POST", ApiKey, ApiSecret, new
    {
        symbol = Symbol,
        type = "TRIGGER_LIMIT",
        side = isLong ? "BUY" : "SELL",
        positionSide = isLong ? "LONG" : "SHORT",
        price,
        stopPrice,
        quantity,
        // recvWindow = 1000,
        takeProfit = JsonSerializer.Serialize(new
        {
            type = "TAKE_PROFIT_MARKET",
            quantity,
            stopPrice = tp,
            price = tp,
            workingType = "MARK_PRICE"
        }),
        stopLoss = JsonSerializer.Serialize(new
        {
            type = "STOP_MARKET",
            quantity,
            stopPrice = sl,
            price = sl,
            workingType = "MARK_PRICE"
        })
    });

    public async Task<HttpResponseMessage> OpenTriggerMarketOrder(bool isLong, float quantity, float stopPrice, float tp, float sl) => await Utilities.HandleRequest("https", Base_Url, "/openApi/swap/v2/trade/order", "POST", ApiKey, ApiSecret, new
    {
        symbol = Symbol,
        type = "TRIGGER_MARKET",
        side = isLong ? "BUY" : "SELL",
        positionSide = isLong ? "LONG" : "SHORT",
        stopPrice,
        quantity,
        // recvWindow = 1000,
        takeProfit = JsonSerializer.Serialize(new
        {
            type = "TAKE_PROFIT_MARKET",
            quantity,
            stopPrice = tp,
            price = tp,
            workingType = "MARK_PRICE"
        }),
        stopLoss = JsonSerializer.Serialize(new
        {
            type = "STOP_MARKET",
            quantity,
            stopPrice = sl,
            price = sl,
            workingType = "MARK_PRICE"
        })
    });

    public async Task CloseOrders() => await Utilities.HandleRequest("https", Base_Url, "/openApi/swap/v2/trade/AllOpenOrders", "DELETE", ApiKey, ApiSecret, new
    {
        symbol = Symbol
    });

    public async Task CloseOrder(long orderId) => await Utilities.HandleRequest("https", Base_Url, "/openApi/swap/v2/trade/order", "DELETE", ApiKey, ApiSecret, new
    {
        symbol = Symbol,
        orderId
    });

    public static float GetTp(bool isLong, float tpPercentage, float lastPrice, int leverage)
    {
        if (isLong)
            return (tpPercentage * lastPrice / (100 * leverage)) + lastPrice;
        else
            return ((tpPercentage * lastPrice / (100 * leverage)) - lastPrice) * -1;
    }

    public static float GetSl(bool isLong, float slPercentage, float lastPrice, int leverage)
    {
        if (isLong)
            return ((slPercentage * lastPrice / (100 * leverage)) - lastPrice) * -1;
        else
            return (slPercentage * lastPrice / (100 * leverage)) + lastPrice;
    }
}
