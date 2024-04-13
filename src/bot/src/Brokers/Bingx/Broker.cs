using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using bot.src.Brokers.Bingx.DTOs;
using bot.src.Brokers.Bingx.Exceptions;
using bot.src.Brokers.Bingx.Models;
using bot.src.Data.Models;
using bot.src.Util;
using ILogger = Serilog.ILogger;

namespace bot.src.Brokers.Bingx;

public class Broker : Api, IBroker
{
    private readonly BrokerOptions _brokerOptions;
    private readonly IBingxUtilities _utilities;
    private readonly ITime _time;
    private readonly ILogger _logger;
    private Candles _candles = new(Array.Empty<Candle>());
    private bool _areCandlesFetched = false;
    private bool _isListeningForCandles = false;
    private int _candlesCount;
    private int _timeFrame;
    private bool _shouldStopListening = false;
    private Candle? _previousCandle;

    public event EventHandler? RestartCandleWebSocket;
    public event EventHandler? RefetchCandles;

    public Broker(IBrokerOptions brokerOptions, IBingxUtilities utilities, ILogger logger, ITime time) : base(brokerOptions)
    {
        _brokerOptions = (brokerOptions as BrokerOptions)!;
        _utilities = utilities;
        _time = time;
        _logger = logger.ForContext<Broker>();

        RestartCandleWebSocket += async (o, e) => await StartCandleWebSocket();
        RefetchCandles += async (o, e) => await FetchCandles();
    }

    private string GetStringTimeFrame(int timeFrame) => timeFrame switch
    {
        60 => "1m",
        60 * 3 => "3m",
        60 * 5 => "5m",
        60 * 15 => "15m",
        60 * 30 => "30m",
        60 * 60 * 1 => "1h",
        60 * 60 * 2 => "2h",
        60 * 60 * 4 => "4h",
        60 * 60 * 6 => "6h",
        60 * 60 * 8 => "8h",
        60 * 60 * 12 => "12h",
        60 * 60 * 24 * 1 => "1d",
        60 * 60 * 24 * 3 => "3d",
        60 * 60 * 24 * 7 => "1w",
        60 * 60 * 24 * 30 => "1M",
        _ => throw new BingxException("Invalid TimeFrame!")
    };

    private async Task FetchCandles()
    {
        _areCandlesFetched = false;

        try
        {
            // In case _candles is empty or it is not empty but is 500 candles old
            if (!_candles.Any() || (_time.GetUtcNow() - _candles.Last().Date).TotalSeconds > (5000 * _timeFrame))
                await FetchHistoricalCandles(_candlesCount, _timeFrame);

            await FetchRecentCandles(_candlesCount, _timeFrame);

            if (_candles.Count() + 3 < _candlesCount)
                throw new BingxException("System failed to fetch enough candles.");

            _areCandlesFetched = true;

            _logger.Information("Finished Candle Store initiation.");
        }
        catch (System.Exception ex)
        {
            _logger.Error(ex, "The broker has failed to fetch historical candles, Restarting...");
            _areCandlesFetched = false;
            RefetchCandles?.Invoke(this, EventArgs.Empty);
        }
    }

    private async Task StartCandleWebSocket()
    {
        if (!_isListeningForCandles && !_shouldStopListening)
            if (!_isListeningForCandles && !_shouldStopListening)
                try { await ListenForCandles(_candlesCount, _timeFrame); }
                catch (MissingCandlesException ex)
                {
                    _logger.Error(ex, "Missing candles detected!");
                    _areCandlesFetched = false;
                    RefetchCandles?.Invoke(this, EventArgs.Empty);

                    _isListeningForCandles = false;
                    RestartCandleWebSocket?.Invoke(this, EventArgs.Empty);
                }
                catch (System.Exception ex)
                {
                    _logger.Error(ex, "The broker's listener has failed, Restarting...");

                    _isListeningForCandles = false;
                    RestartCandleWebSocket?.Invoke(this, EventArgs.Empty);
                }
    }

    public Task InitiateCandleStore(int? candlesCount = null, int? timeFrame = null)
    {
        if (candlesCount == null)
            throw new ArgumentException(null, paramName: nameof(candlesCount));

        _candlesCount = (int)candlesCount;

        // Make public able to use other arbitrary time frames.
        timeFrame ??= _brokerOptions.TimeFrame;
        _timeFrame = (int)timeFrame;

        _logger.Information("Initiating Candle Store...");

        Parallel.Invoke(
            async () => await FetchCandles(),
            async () => await StartCandleWebSocket()
        );

        return Task.CompletedTask;
    }

    private async Task FetchHistoricalCandles(int candlesCount, int timeFrame)
    {
        _logger.Information("Fetching historical candles...");
        _logger.Information($"{nameof(candlesCount)}: {{candlesCount}}", candlesCount);

        if (candlesCount < 1)
            throw new BingxException();

        long? endTime = null;
        List<Candle> candles = new();
        while (candles.Count < candlesCount)
        {
            _logger.Information($"{nameof(candles)} Count: {{candlesCount}}", candles.Count);
            IEnumerable<BingxCandle> bingxCandles = await GetKline(GetStringTimeFrame(timeFrame), Math.Min(1400, candlesCount), null, endTime);

            List<Candle> convertedCandles = bingxCandles.Reverse().ToList().ConvertAll(o => new Candle()
            {
                Open = decimal.Parse(o.Open),
                Close = decimal.Parse(o.Close),
                High = decimal.Parse(o.High),
                Low = decimal.Parse(o.Low),
                Volume = decimal.Parse(o.Volume),
                Date = DateTimeOffset.FromUnixTimeMilliseconds(o.CloseTime).DateTime,
            });

            if (!bingxCandles.Any() || (candles.Any() && candles.First().Date <= DateTimeOffset.FromUnixTimeMilliseconds(bingxCandles.Last().CloseTime).DateTime))
                bingxCandles = bingxCandles.SkipLast(1);
            if (!bingxCandles.Any() || (candles.Any() && candles.First().Date <= DateTimeOffset.FromUnixTimeMilliseconds(bingxCandles.Last().CloseTime).DateTime))
                throw new BingxException("Server does not provide new candles.");

            if (candles.Any())
                for (int i = 0; i < bingxCandles.Count(); i++)
                    if (DateTimeOffset.FromUnixTimeMilliseconds(bingxCandles.ElementAt(i).CloseTime).DateTime == candles.First().Date)
                    {
                        bingxCandles = bingxCandles.Skip(i + 1);
                        break;
                    }

            if ((candles.Count + bingxCandles.Count()) > candlesCount)
                bingxCandles = bingxCandles.Take(candlesCount - candles.Count);

            endTime = bingxCandles.Last().CloseTime;

            convertedCandles.AddRange(candles);
            candles = convertedCandles;
            _logger.Information($"{nameof(candles)}: {{candlesCount}}", candles.Count);
        }

        _candles = new Candles(candles);
        _candles.SkipLast(2);

        try
        {
            File.WriteAllText($"{Program.RootPath}/{Symbol}_HistoricalCandles_{GetStringTimeFrame(timeFrame)}.json", JsonSerializer.Serialize(_candles, new JsonSerializerOptions() { WriteIndented = true }));
        }
        catch (System.Exception ex) { _logger.Error(ex, "Failure when trying to store fetched candles in hard dick."); }

        _logger.Information("Finished fetching historical candles...");
    }

    public async Task FetchRecentCandles(int candlesCount, int timeFrame)
    {
        _logger.Information("Fetching recent candles...");
        _logger.Information($"{nameof(candlesCount)}: {{candlesCount}}", candlesCount);

        _logger.Information("Last candle's date: {lastCandleDate}", _candles.Last().Date);

        long startTime = DateTimeOffset.Parse(_candles.Last().Date.ToString()).ToUnixTimeMilliseconds();
        DateTime now = _time.GetUtcNow();

        if (now.Second >= 30)
        {
            _logger.Debug("Waiting until zero second.");
            Task.Delay((60 - now.Second) * 1000).GetAwaiter().GetResult();
            now = _time.GetUtcNow();
        }

        while ((now - _candles.Last().Date.AddSeconds(_brokerOptions.TimeFrame)).TotalSeconds >= _brokerOptions.TimeFrame)
        {
            Task.Delay(1000).GetAwaiter().GetResult();

            IEnumerable<BingxCandle> bingxCandles = await GetKline(GetStringTimeFrame(timeFrame), Math.Min(1400, candlesCount), startTime);

            if (!bingxCandles.Any() || (_candles.Any() && _candles.Last().Date >= DateTimeOffset.FromUnixTimeMilliseconds(bingxCandles.First().CloseTime).DateTime))
                throw new BingxException("Server does not provide new candles.");

            if (_candles.Any() && _candles.Last().Date == DateTimeOffset.FromUnixTimeMilliseconds(bingxCandles.Last().CloseTime).DateTime)
                bingxCandles = bingxCandles.SkipLast(1);
            else
                for (int i = 0; i < bingxCandles.Count(); i++)
                    if (DateTimeOffset.FromUnixTimeMilliseconds(bingxCandles.ElementAt(i).CloseTime).DateTime == _candles.Last().Date)
                    {
                        bingxCandles = bingxCandles.Take(i);
                        break;
                    }

            // Skip most recent candle if it's not closed yet
            while (bingxCandles.Any() && (_time.GetUtcNow() - DateTimeOffset.FromUnixTimeMilliseconds(bingxCandles.First().CloseTime).DateTime).TotalSeconds < _brokerOptions.TimeFrame)
                bingxCandles = bingxCandles.Skip(1);

            if (!bingxCandles.Any())
                break;

            startTime = bingxCandles.First().CloseTime;

            List<Candle> convertedCandles = bingxCandles
                        .Reverse()
                        .ToList()
                        .ConvertAll(o => new Candle()
                        {
                            Open = decimal.Parse(o.Open),
                            Close = decimal.Parse(o.Close),
                            High = decimal.Parse(o.High),
                            Low = decimal.Parse(o.Low),
                            Volume = decimal.Parse(o.Volume),
                            Date = DateTimeOffset.FromUnixTimeMilliseconds(o.CloseTime).DateTime
                        });

            _logger.Debug("convertedCandlesCount: {convertedCandlesCount}", convertedCandles.Count);

            for (int i = 0; i < convertedCandles.Count; i++)
            {
                _logger.Debug("i: {i}", i);
                if (Math.Abs((_candles.Last().Date.AddSeconds(_brokerOptions.TimeFrame) - convertedCandles.ElementAt(i).Date).TotalSeconds) < 5)
                    _candles.Add(convertedCandles.ElementAt(i));
            }

            now = _time.GetUtcNow();

            if (now.Second >= 30)
            {
                await Task.Delay((60 - now.Second) * 1000);
                now = _time.GetUtcNow();
            }
        }

        _candles.TakeLast(_candlesCount);

        _candles.TakeLast(_candlesCount);

        _logger.Information("Last candle's date: {lastCandleDate}", _candles.Last().Date);
        _logger.Information("Finished fetching recent candles...");
    }

    public async Task ListenForCandles(int candlesCount, int timeFrame)
    {
        try
        {
            _logger.Information("Listening for candles...");

            ClientWebSocket ws = new();

            await ws.ConnectAsync(new Uri("wss://open-api-swap.bingx.com/swap-market"), CancellationToken.None);

            string id = Guid.NewGuid().ToString();
            while (true)
            {
                if (ws.State != WebSocketState.Open)
                    continue;

                await ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new
                {
                    id,
                    reqType = "sub",
                    dataType = $"{Symbol}@kline_{GetStringTimeFrame(timeFrame)}"
                }))), WebSocketMessageType.Text, true, CancellationToken.None);

                break;
            }

            _previousCandle = null;
            while (true)
            {
                if (ws.State == WebSocketState.None || ws.State == WebSocketState.Connecting)
                    continue;

                if (ws.State == WebSocketState.Closed || ws.State == WebSocketState.Aborted || ws.State == WebSocketState.CloseReceived)
                {
                    _logger.Information("Websocket state: {state}", ws.State);
                    break;
                }

                if (_shouldStopListening)
                    break;

                if (!_isListeningForCandles)
                    _isListeningForCandles = true;

                byte[] buffer = new byte[1024 * 4];
                WebSocketReceiveResult result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                byte[] bytes = await _utilities.DecompressBytes(buffer);

                string message = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

                if (message == "Ping")
                {
                    _logger.Verbose(message);

                    await ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("Pong")), WebSocketMessageType.Binary, true, CancellationToken.None);
                    buffer = new byte[1024 * 4];
                    continue;
                }

                BingxWsResponse? wsResponse = JsonSerializer.Deserialize<BingxWsResponse>(message, new JsonSerializerOptions(JsonSerializerDefaults.Web));

                if (wsResponse != null && wsResponse.Id != null && wsResponse.Id != id)
                    throw new BingxException("Invalid id provided by the server.");

                if (wsResponse == null || wsResponse.Data == null || !wsResponse.Data.Any())
                    continue;

                Candle candle = new()
                {
                    Open = decimal.Parse(wsResponse.Data!.First().o),
                    Close = decimal.Parse(wsResponse.Data!.First().c),
                    High = decimal.Parse(wsResponse.Data!.First().h),
                    Low = decimal.Parse(wsResponse.Data!.First().l),
                    Date = DateTimeOffset.FromUnixTimeMilliseconds(wsResponse.Data!.First().T).DateTime,
                    Volume = decimal.Parse(wsResponse.Data!.First().v)
                };

                if (!_areCandlesFetched)
                {
                    _previousCandle = candle;
                    continue;
                }

                if (_previousCandle == null || _previousCandle.Date == candle.Date)
                {
                    _previousCandle = candle;
                    continue;
                }

                if (Math.Abs((_candles.Last()!.Date.AddSeconds(_brokerOptions.TimeFrame) - _previousCandle.Date).TotalSeconds) > 5)
                {
                    _logger.Information("Last candle close date: {lastCandleDate}", _candles.Last()!.Date.AddSeconds(_brokerOptions.TimeFrame));
                    _logger.Information("Closed candle open date: {closedCandleDate}", _previousCandle.Date);

                    _areCandlesFetched = false;

                    throw new MissingCandlesException();
                }

                if (_previousCandle.Date != _candles.Last().Date)
                {
                    _candles.Add(_previousCandle);
                    _logger.Information("new candle added: {@previousCandle} at: {time}", _previousCandle, _time.GetUtcNow());
                    _logger.Information("current candle: {@candle}", candle);
                    _logger.Information("Candles count: {count}", _candles.Count());
                }

                if (candlesCount < _candles.Count())
                    _candles.Skip(_candles.Count() - candlesCount);

                _previousCandle = candle;
            }

            await ws.CloseAsync(WebSocketCloseStatus.Empty, null, CancellationToken.None);

            _logger.Information("Finished listening for candles...");
        }
        finally
        {
            _isListeningForCandles = false;
            _shouldStopListening = false;
        }
    }

    public void StopListening()
    {
        if (_isListeningForCandles)
            _shouldStopListening = true;
    }

    public void StartListening()
    {
        if (!_isListeningForCandles)
            RestartCandleWebSocket?.Invoke(this, EventArgs.Empty);
    }

    public async Task<Candles?> GetCandles(int? timeFrameSeconds = null)
    {
        timeFrameSeconds ??= _brokerOptions.TimeFrame;

        if (!_areCandlesFetched)
            return null;

        if (!_candles.Any())
            await InitiateCandleStore(timeFrame: timeFrameSeconds);

        return _candles;
    }

    public Task<Candle?> GetCandle(int indexFromEnd = 0)
    {
        try
        {
            if (_candles == null || !_candles.Any())
                return Task.FromResult<Candle?>(null);
            return Task.FromResult<Candle?>(_candles.ElementAt(_candles.Count() - indexFromEnd - 1));
        }
        catch (System.Exception) { return Task.FromResult<Candle?>(null); }
    }

    public async Task<int?> GetLastCandleIndex()
    {
        Candles? candles = await GetCandles();
        if (candles == null)
            return null;
        return candles.Count() - 1;
    }

    public async Task<decimal> GetLastPrice()
    {
        decimal lastPrice;
        if (_previousCandle != null)
        {
            _logger.Information("Getting last price of the symbol...");
            lastPrice = _previousCandle.Close;
            _logger.Information("Got symbol's last price: {price}", _previousCandle.Close);
            return lastPrice;
        }
        else
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

            lastPrice = decimal.Parse(data!["price"]!.Value.GetString()!);

            _logger.Information("Got symbol's last price: {price}", lastPrice);
            return lastPrice;
        }
    }

    public Task CandleClosed() => throw new NotImplementedException();

    public async Task CloseAllPositions()
    {
        _logger.Information("Closing all the open positions...");

        HttpResponseMessage httpResponseMessage = await _utilities.HandleBingxRequest("https", Base_Url, "/openApi/swap/v2/trade/closeAllPositions", "POST", ApiKey, ApiSecret, new { });

        if (!await _utilities.TryEnsureSuccessfulBingxResponse(httpResponseMessage))
            throw new CloseAllPositionsException();

        _logger.Information("Finished Closing all the open positions...");
        return;
    }

    private async Task<IEnumerable<BingxCandle>> GetKline(string interval, int? limit, long? startTime = null, long? endTime = null)
    {
        HttpResponseMessage httpResponseMessage = await _utilities.HandleBingxRequest("https", Base_Url, "/openApi/swap/v1/market/markPriceKlines", "GET", ApiKey, ApiSecret, new
        {
            symbol = Symbol,
            interval,
            limit,
            startTime,
            endTime
        });

        string response = await httpResponseMessage.Content.ReadAsStringAsync();
        BingxResponse<IEnumerable<BingxCandle>> bingxResponse;
        try
        {
            bingxResponse = JsonSerializer.Deserialize<BingxResponse<IEnumerable<BingxCandle>>>(response, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new BingxException("Failure while trying to fetch historical candles.");
        }
        catch (System.Exception ex)
        {
            _logger.Error(ex, "The broker failed: {message}", ex.Message);
            _logger.Information("The response: {response}", response);
            throw;
        }

        if (bingxResponse.Data == null)
            throw new BingxException("Failure while trying to fetch historical candles.");

        return bingxResponse.Data;
    }

    public Task<IEnumerable<Position?>> GetClosedPositions(DateTime start, DateTime? end = null)
    {
        throw new NotImplementedException();
    }

    public async Task<int> GetOpenPositionsCount()
    {
        _logger.Information("Getting all the open positions...");

        HttpResponseMessage httpResponseMessage = await _utilities.HandleBingxRequest("https", Base_Url, "/openApi/swap/v2/user/positions", "GET", ApiKey, ApiSecret, new
        {
            symbol = Symbol
        });

        if (!await _utilities.TryEnsureSuccessfulBingxResponse(httpResponseMessage))
            throw new CloseAllPositionsException();

        string json = await httpResponseMessage.Content.ReadAsStringAsync();
        BingxResponse<IEnumerable<BingxPositionDto>> bingxResponse = JsonSerializer.Deserialize<BingxResponse<IEnumerable<BingxPositionDto>>>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new CloseAllPositionsException();

        _logger.Information("Finished getting all the open positions...");
        return bingxResponse.Data!.Count();
    }

    public async Task<IEnumerable<Position?>> GetOpenPositions()
    {
        _logger.Information("Getting all the open positions...");

        HttpResponseMessage httpResponseMessage = await _utilities.HandleBingxRequest("https", Base_Url, "/openApi/swap/v2/user/positions", "GET", ApiKey, ApiSecret, new
        {
            symbol = Symbol
        });

        if (!await _utilities.TryEnsureSuccessfulBingxResponse(httpResponseMessage))
            throw new CloseAllPositionsException();

        string json = await httpResponseMessage.Content.ReadAsStringAsync();
        BingxResponse<IEnumerable<BingxPositionDto>> bingxResponse = JsonSerializer.Deserialize<BingxResponse<IEnumerable<BingxPositionDto>>>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new CloseAllPositionsException();

        _logger.Information("Finished getting all the open positions...");
        return bingxResponse.Data!.ToList().ConvertAll(bp => new Position()
        {
            Id = bp.PositionId,
            Symbol = bp.Symbol,
            PositionDirection = PositionDirection.Parse(bp.PositionSide),
            OpenedAt = DateTimeOffset.FromUnixTimeMilliseconds(bp.UpdateTime).DateTime,
            CreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(bp.UpdateTime).DateTime
        });
    }

    public async Task OpenMarketPosition(decimal entryPrice, decimal margin, decimal leverage, string direction, decimal slPrice)
    {
        await SetLeverage((int)leverage, direction == PositionDirection.LONG);
        _logger.Information("Opening a market order...");
        _logger.Information("entryPrice: {entryPrice}, margin: {margin}, leverage: {leverage}, direction: {direction}, slPrice: {slPrice}", entryPrice, margin, leverage, direction, slPrice);

        decimal quantity = margin * leverage / entryPrice;

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

    public async Task OpenMarketPosition(decimal entryPrice, decimal margin, decimal leverage, string direction, decimal slPrice, decimal tpPrice)
    {
        await SetLeverage((int)leverage, direction == PositionDirection.LONG);
        _logger.Information("Opening a market order...");
        _logger.Information("entryPrice: {entryPrice}, margin: {margin}, leverage: {leverage}, direction: {direction}, slPrice: {slPrice}", entryPrice, margin, leverage, direction, slPrice);

        decimal quantity = margin * leverage / entryPrice;

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
            }),
            takeProfit = JsonSerializer.Serialize(new
            {
                type = "TAKE_PROFIT_MARKET",
                quantity,
                stopPrice = tpPrice,
                price = tpPrice,
                workingType = "MARK_PRICE"
            })
        });

        if (!await _utilities.TryEnsureSuccessfulBingxResponse(httpResponseMessage))
            throw new OpenMarketOrderException();

        _logger.Information("Finished opening market order...");
        return;
    }

    private async Task<int> GetLeverage(bool direction)
    {
        _logger.Information("Getting the leverage...");

        HttpResponseMessage httpResponseMessage = await _utilities.HandleBingxRequest("https", Base_Url, "/openApi/swap/v2/trade/leverage", "GET", ApiKey, ApiSecret, new
        {
            symbol = Symbol
        });

        if (!await _utilities.TryEnsureSuccessfulBingxResponse(httpResponseMessage))
            throw new SetLeverageException();

        string response = await httpResponseMessage.Content.ReadAsStringAsync();

        BingxLeverageDto bingxLeverageDto = (JsonSerializer.Deserialize<BingxResponse<BingxLeverageDto>>(response, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new SetLeverageException()).Data ?? throw new SetLeverageException();

        int leverage = direction ? bingxLeverageDto.LongLeverage : bingxLeverageDto.ShortLeverage;

        _logger.Information("Leverage => {leverage}", leverage);

        _logger.Information("Finished getting leverage...");
        return leverage;
    }

    private async Task SetLeverage(int leverage, bool direction)
    {
        _logger.Information("Setting the leverage...");
        _logger.Information("leverage: {leverage}", leverage);

        HttpResponseMessage httpResponseMessage = await _utilities.HandleBingxRequest("https", Base_Url, "/openApi/swap/v2/trade/leverage", "POST", ApiKey, ApiSecret, new
        {
            symbol = Symbol,
            side = direction ? LONG_SIDE : SHORT_SIDE,
            leverage
        });

        if (!await _utilities.TryEnsureSuccessfulBingxResponse(httpResponseMessage))
            throw new SetLeverageException();

        _logger.Information("Finished setting leverage...");
        return;
    }

    public async Task CancelAllPendingPositions()
    {
        _logger.Information("Closing all the open positions...");

        HttpResponseMessage httpResponseMessage = await _utilities.HandleBingxRequest("https", Base_Url, "/openApi/swap/v2/trade/allOpenOrders", "DELETE", ApiKey, ApiSecret, new { symbol = Symbol });

        if (!await _utilities.TryEnsureSuccessfulBingxResponse(httpResponseMessage))
            throw new CloseAllPositionsException();

        _logger.Information("Finished Closing all the open positions...");
        return;
    }

    public decimal GetBalance() => _brokerOptions.AccountOptions.Balance;

    public Task ClosePosition(Position position) => throw new NotImplementedException();

    public Task CancelPosition(string id, DateTime cancelledAt) => throw new NotImplementedException();

    public Task<IEnumerable<Position?>> GetPendingPositions() => throw new NotImplementedException();

    public Task CancelAllLongPendingPositions() => throw new NotImplementedException();

    public Task CancelAllShortPendingPositions() => throw new NotImplementedException();

    public Task OpenLimitPosition(decimal entryPrice, decimal margin, decimal leverage, string direction, decimal limit, decimal slPrice) => throw new NotImplementedException();

    public Task OpenLimitPosition(decimal entryPrice, decimal margin, decimal leverage, string direction, decimal limit, decimal slPrice, decimal tpPrice) => throw new NotImplementedException();
}
