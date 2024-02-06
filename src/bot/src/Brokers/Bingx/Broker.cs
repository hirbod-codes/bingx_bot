using System.ComponentModel;
using System.Text.Json;
using bot.src.Brokers.Bingx.DTOs;
using bot.src.Brokers.Bingx.Exceptions;
using bot.src.Brokers.Bingx.Models;
using bot.src.Data.Models;
using Serilog;

namespace bot.src.Brokers.Bingx;

public class Broker : Api, IBroker
{
    private readonly ITrade _trade;
    private readonly IBingxUtilities _utilities;
    private readonly ILogger _logger;

    public Broker(IBrokerOptions brokerOptions, ITrade trade, IBingxUtilities utilities, ILogger logger) : base(brokerOptions)
    {
        _trade = trade;
        _utilities = utilities;
        _logger = logger.ForContext<Trade>();
    }

    public async Task<decimal> GetLastPrice()
    {
        try
        {
            _logger.Information("Getting last price of the symbol...");

            HttpResponseMessage httpResponseMessage = await _utilities.HandleBingxRequest("https", Base_Url, "/openApi/swap/v2/quote/price", "GET", ApiKey, ApiSecret, new
            {
                symbol = Symbol,
            });
            await _utilities.EnsureSuccessfulBingxResponse(httpResponseMessage);

            string response = await httpResponseMessage.Content.ReadAsStringAsync();

            Dictionary<string, JsonElement?>? dictionary = JsonSerializer.Deserialize<Dictionary<string, JsonElement?>>(response);

            Dictionary<string, JsonElement?>? data = JsonSerializer.Deserialize<Dictionary<string, JsonElement?>>(dictionary!["data"]!.Value);

            decimal lastPrice = decimal.Parse(data!["price"]!.Value.GetString()!);
            _logger.Information($"last price => {lastPrice}");

            _logger.Information("Got last symbol price...");
            return lastPrice;
        }
        catch (LastPriceException ex)
        {
            _logger.Error(ex, "Failure while trying to get the {symbol} last price", Symbol);
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure while trying to get the {symbol} last price", Symbol);
            throw new LastPriceException();
        }
    }

    public Task CandleClosed()
    {
        throw new NotImplementedException();
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

    public Task<Candle?> GetCandle(int index)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Position>> GetClosedPositions(DateTime start, DateTime? end = null)
    {
        throw new NotImplementedException();
    }

    public Task<Candle> GetCurrentCandle()
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Position>> GetOpenPositions()
    {
        _logger.Information("Closing all the open positions...");

        HttpResponseMessage httpResponseMessage = await _utilities.HandleBingxRequest("https", Base_Url, "/openApi/swap/v2/trade/allOrders", "GET", ApiKey, ApiSecret, new
        {
            symbol = Symbol,
            startTime = DateTimeOffset.Parse(DateTime.UtcNow.AddHours(-5).ToString()).ToUnixTimeMilliseconds()
        });

        if (!await _utilities.TryEnsureSuccessfulBingxResponse(httpResponseMessage))
            throw new CloseAllPositionsException();

        BingxResponse<IEnumerable<BingxPositionDto>> bingxResponse = JsonSerializer.Deserialize<BingxResponse<IEnumerable<BingxPositionDto>>>(await httpResponseMessage.Content.ReadAsStringAsync(), new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new CloseAllPositionsException();

        _logger.Information("Finished Closing all the open positions...");
        return bingxResponse.Data!.ToList().ConvertAll(bp => new Position()
        {
            Id = bp.PositionId,
            Symbol = bp.Symbol,
            // OpenedPrice = decimal.Parse(bp.AvgPrice),
            // ClosedPrice = null,
            // SLPrice = bp.,
            // TPPrice = bp.,
            // CommissionRatio = bp.,
            // Commission = bp.,
            // Profit = bp.,
            // ProfitWithCommission = bp.,
            // Margin = bp.,
            // Leverage = bp.,
            // PositionStatus = bp.,
            // PositionDirection = bp.,
            // OpenedAt = bp.,
            // ClosedAt = bp.,
        });
    }

    public async Task OpenMarketPosition(decimal margin, decimal leverage, string direction, decimal slPrice)
    {
        await SetLeverage((int)leverage);
        _logger.Information("Opening a market order...");
        _logger.Information("margin: {margin}, leverage: {leverage}, direction: {direction}, slPrice: {slPrice}", margin, leverage, direction, slPrice);

        decimal quantity = margin * leverage / await GetLastPrice();

        HttpResponseMessage httpResponseMessage = await _utilities.HandleBingxRequest("https", Base_Url, "/openApi/swap/v2/trade/order", "POST", ApiKey, ApiSecret, new
        {
            symbol = Symbol,
            type = "MARKET",
            side = direction == PositionDirection.LONG ? "BUY" : "SELL",
            positionSide = direction == PositionDirection.LONG ? "LONG" : "SHORT",
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

    public Task OpenMarketPosition(decimal margin, decimal leverage, string direction, decimal slPrice, decimal tpPrice)
    {
        throw new NotImplementedException();
    }

    public Task SetCurrentCandle(Candle candle)
    {
        throw new NotImplementedException();
    }

    private async Task<int> GetLeverage()
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

    private async Task SetLeverage(int leverage)
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
}
