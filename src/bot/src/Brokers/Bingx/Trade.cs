using System.Runtime.Serialization;
using System.Text.Json;
using bot.src.Brokers.Bingx.DTOs;
using bot.src.Brokers.Bingx.Exceptions;
using bot.src.Brokers.Bingx.Models;
using bot.src.Strategies.Models;
using Serilog;

namespace bot.src.Brokers.Bingx;

public class Trade : Api, ITrade
{
    private readonly IBingxUtilities _utilities;
    private readonly ILogger _logger;

    public Trade(string base_url, string apiKey, string apiSecret, string symbol, IBingxUtilities utilities, ILogger logger) : base(base_url, apiKey, apiSecret, symbol)
    {
        _utilities = utilities;
        _logger = logger.ForContext<Trade>();
    }

    public Task<string> GetSymbol() => Task.FromResult(Symbol);

    public Task SetSymbol(string symbol)
    {
        Symbol = symbol;
        return Task.CompletedTask;
    }

    public async Task<float> GetPrice()
    {
        _logger.Information("Getting last price of the symbol...");

        HttpResponseMessage httpResponseMessage = await _utilities.HandleBingxRequest("https", Base_Url, "/openApi/swap/v2/quote/price", "GET", ApiKey, ApiSecret, new
        {
            symbol = Symbol,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });

        if (!await _utilities.TryEnsureSuccessfulBingxResponse(httpResponseMessage))
            throw new LastPriceException();

        string response = await httpResponseMessage.Content.ReadAsStringAsync();

        if (!float.TryParse(((JsonSerializer.Deserialize<BingxResponse<BingxLastPriceDto>>(response, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new LastPriceException()).Data ?? throw new LastPriceException()).Price ?? throw new LastPriceException(), out float price))
            throw new LastPriceException();

        _logger.Information("symbol's last price => {price}", price);

        _logger.Information("Finished getting symbol's last price...");
        return price;
    }

    public async Task<float> GetLeverage()
    {
        _logger.Information("Getting the leverage...");

        HttpResponseMessage httpResponseMessage = await _utilities.HandleBingxRequest("https", Base_Url, "/openApi/swap/v2/trade/leverage", "GET", ApiKey, ApiSecret, new
        {
            symbol = Symbol
        });

        if (!await _utilities.TryEnsureSuccessfulBingxResponse(httpResponseMessage))
            throw new SetLeverageException();

        string response = await httpResponseMessage.Content.ReadAsStringAsync();

        int leverage = ((JsonSerializer.Deserialize<BingxResponse<BingxLeverageDto>>(response, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new SetLeverageException()).Data ?? throw new SetLeverageException()).LongLeverage;

        _logger.Information("Leverage => {leverage}", leverage);

        _logger.Information("Finished getting leverage...");
        return leverage;
    }

    public async Task SetLeverage(float leverage)
    {
        _logger.Information("Setting the leverage...");
        _logger.Information("leverage: {leverage}", leverage);

        Task<HttpResponseMessage> task1 = _utilities.HandleBingxRequest("https", Base_Url, "/openApi/swap/v2/trade/leverage", "POST", ApiKey, ApiSecret, new
        {
            symbol = Symbol,
            side = LONG_SIDE,
            leverage
        });

        Task<HttpResponseMessage> task2 = _utilities.HandleBingxRequest("https", Base_Url, "/openApi/swap/v2/trade/leverage", "POST", ApiKey, ApiSecret, new
        {
            symbol = Symbol,
            side = SHORT_SIDE,
            leverage
        });

        await Task.WhenAll(task1, task2);

        if (!task1.IsCompletedSuccessfully || !task2.IsCompletedSuccessfully)
            throw new SetLeverageException();

        if (!await _utilities.TryEnsureSuccessfulBingxResponse(task1.Result) || !await _utilities.TryEnsureSuccessfulBingxResponse(task2.Result))
            throw new SetLeverageException();

        _logger.Information("Finished setting leverage...");
        return;
    }

    public async Task OpenMarketOrder(float quantity, bool direction, float slPrice, float tpPrice)
    {
        _logger.Information("Opening a market order...");
        _logger.Information("quantity: {quantity}, direction: {direction}, slPrice: {slPrice}, tpPrice: {tpPrice}", quantity, direction, slPrice, tpPrice);

        HttpResponseMessage httpResponseMessage = await _utilities.HandleBingxRequest("https", Base_Url, "/openApi/swap/v2/trade/order", "POST", ApiKey, ApiSecret, new
        {
            symbol = Symbol,
            type = "MARKET",
            side = direction ? "BUY" : "SELL",
            positionSide = direction ? "LONG" : "SHORT",
            quantity,
            takeProfit = JsonSerializer.Serialize(new
            {
                type = "TAKE_PROFIT_MARKET",
                quantity,
                stopPrice = tpPrice,
                price = tpPrice,
                workingType = "MARK_PRICE"
            }),
            stopLoss = JsonSerializer.Serialize(new
            {
                type = "STOP_MARKET",
                quantity,
                stopPrice = slPrice,
                price = slPrice,
                workingType = "MARK_PRICE"
            })
        });

        if (!await _utilities.TryEnsureSuccessfulBingxResponse(httpResponseMessage))
            throw new OpenMarketOrderException();

        _logger.Information("Finished opening market order...");
        return;
    }

    public async Task OpenMarketOrder(float quantity, bool direction, float slPrice)
    {
        _logger.Information("Opening a market order...");
        _logger.Information("quantity: {quantity}, direction: {direction}, slPrice: {slPrice}", quantity, direction, slPrice);

        HttpResponseMessage httpResponseMessage = await _utilities.HandleBingxRequest("https", Base_Url, "/openApi/swap/v2/trade/order", "POST", ApiKey, ApiSecret, new
        {
            symbol = Symbol,
            type = "MARKET",
            side = direction ? "BUY" : "SELL",
            positionSide = direction ? "LONG" : "SHORT",
            quantity,
            stopLoss = JsonSerializer.Serialize(new
            {
                type = "STOP_MARKET",
                quantity,
                stopPrice = slPrice,
                price = slPrice,
                workingType = "MARK_PRICE"
            })
        });

        if (!await _utilities.TryEnsureSuccessfulBingxResponse(httpResponseMessage))
            throw new OpenMarketOrderException();

        _logger.Information("Finished opening market order...");
        return;
    }

    public async Task CloseAllPositions()
    {
        _logger.Information("Closing all the open positions...");

        HttpResponseMessage httpResponseMessage = await _utilities.HandleBingxRequest("https", Base_Url, "/openApi/swap/v2/trade/closeAllPositions", "POST", ApiKey, ApiSecret, new { });

        if (!await _utilities.TryEnsureSuccessfulBingxResponse(httpResponseMessage))
            throw new CloseAllPositionsException();

        _logger.Information("Finished Closing all the open positions...");
        return;
    }

    public async Task CloseAllPositions(IEnumerable<string> ids)
    {
        _logger.Information("Closing all open positions...");
        _logger.Information("ids: {ids}", string.Join(",", ids));

        HttpResponseMessage httpResponseMessage = await _utilities.HandleBingxRequest("https", Base_Url, "/openApi/swap/v2/trade/batchOrders", "DELETE", ApiKey, ApiSecret, new
        {
            symbol = Symbol,
            orderIdList = ids
        });

        if (!await _utilities.TryEnsureSuccessfulBingxResponse(httpResponseMessage))
            throw new CloseAllPositionsException();

        _logger.Information("Finished Closing all open positions...");
        return;
    }

    public async Task ClosePosition(string id)
    {
        _logger.Information("Closing an open position...");

        HttpResponseMessage httpResponseMessage = await _utilities.HandleBingxRequest("https", Base_Url, "/openApi/swap/v2/trade/order", "DELETE", ApiKey, ApiSecret, new
        {
            symbol = Symbol,
            orderId = id
        });

        if (!await _utilities.TryEnsureSuccessfulBingxResponse(httpResponseMessage))
            throw new CloseAllPositionsException();

        _logger.Information("Finished Closing an open position...");
        return;
    }

    public async Task<Position> GetPosition(string id)
    {
        _logger.Information("Getting a position...");

        HttpResponseMessage httpResponseMessage = await _utilities.HandleBingxRequest("https", Base_Url, "/openApi/swap/v2/trade/order", "GET", ApiKey, ApiSecret, new
        {
            symbol = Symbol,
            orderId = id
        });

        if (!await _utilities.TryEnsureSuccessfulBingxResponse(httpResponseMessage))
            throw new CloseAllPositionsException();

        string response = await httpResponseMessage.Content.ReadAsStringAsync();

        BingxResponse<BingxPositionDto> bingxResponse = JsonSerializer.Deserialize<BingxResponse<BingxPositionDto>>(response, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new CloseAllPositionsException();

        Position position;

        throw new NotImplementedException();
        // try
        // {
        //     position = new()
        //     {
        //         EntryPrice = float.Parse((bingxResponse.Data ?? throw new CloseAllPositionsException()).Price),
        //         SLPrice = float.Parse((bingxResponse.Data ?? throw new CloseAllPositionsException())),
        //         TPPrice = "",
        //         Commission = "",
        //         Leverage = "",
        //         OpenedAt = DateTimeOffset.FromUnixTimeMilliseconds((bingxResponse.Data ?? throw new CloseAllPositionsException()).Time).DateTime,
        //         ClosedAt = DateTimeOffset.FromUnixTimeMilliseconds((bingxResponse.Data ?? throw new CloseAllPositionsException()).Time).DateTime,
        //     };
        // }
        // catch (CloseAllPositionsException) { throw; }
        // catch (Exception) { throw new CloseAllPositionsException(); }

        _logger.Information("position: {@position}", position);
        _logger.Information("Finished getting a position...");
        return position;
    }

    public async Task<IEnumerable<Position>> GetOpenPositions()
    {
        // /openApi/swap/v2/trade/allOrders
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Position>> GetOpenPositions(DateTime start, DateTime? end = null)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Position>> GetClosedPositions()
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Position>> GetClosedPositions(DateTime start, DateTime? end = null)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Position>> GetAllPositions()
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Position>> GetAllPositions(DateTime start, DateTime? end = null)
    {
        throw new NotImplementedException();
    }
}

[Serializable]
internal class CloseAllPositionsException : Exception
{
    public CloseAllPositionsException()
    {
    }

    public CloseAllPositionsException(string? message) : base(message)
    {
    }

    public CloseAllPositionsException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected CloseAllPositionsException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}

[Serializable]
internal class OpenMarketOrderException : Exception
{
    public OpenMarketOrderException()
    {
    }

    public OpenMarketOrderException(string? message) : base(message)
    {
    }

    public OpenMarketOrderException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected OpenMarketOrderException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
