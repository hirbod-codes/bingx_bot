using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using bot.src.Brokers.Bingx.DTOs;
using bot.src.Brokers.Bingx.Exceptions;
using bot.src.Brokers.Bingx.Models;
using bot.src.Data.Models;
using Serilog;

namespace bot.src.Brokers.Bingx;

public class Broker : Api, IBroker
{
    private readonly BrokerOptions _brokerOptions;
    private readonly IBingxUtilities _utilities;
    private readonly ILogger _logger;
    private Candles _candles = null!;
    private bool _areCandlesFetched = false;
    private bool _isListeningForCandles = false;
    private bool _areCandlesLocked = false;

    public Broker(IBrokerOptions brokerOptions, IBingxUtilities utilities, ILogger logger) : base(brokerOptions)
    {
        _brokerOptions = (brokerOptions as BrokerOptions)!;
        _utilities = utilities;
        _logger = logger.ForContext<Broker>();
    }

    public async Task<decimal> GetLastPrice()
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

    private async Task<IEnumerable<BingxCandle>> GetKline(string interval, int? limit, long? startTime = null, long? endTime = null)
    {
        HttpResponseMessage httpResponseMessage = await _utilities.HandleBingxRequest("https", Base_Url, "/openApi/swap/v3/quote/klines", "GET", ApiKey, ApiSecret, new
        {
            symbol = Symbol,
            interval,
            limit,
            startTime,
            endTime
        });

        string response = await httpResponseMessage.Content.ReadAsStringAsync();
        BingxResponse<IEnumerable<BingxCandle>> bingxResponse = JsonSerializer.Deserialize<BingxResponse<IEnumerable<BingxCandle>>>(response, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new BingxException("Failure while trying to fetch historical candles.");

        if (bingxResponse.Data == null)
            throw new BingxException("Failure while trying to fetch historical candles.");

        return bingxResponse.Data;
    }

    public async Task InitiateCandleStore(int candlesCount = 10000, int? timeFrame = null)
    {
        timeFrame ??= _brokerOptions.TimeFrame;

        _logger.Information("Initiating Candle Store...");

        RunConcurrently(ListenForCandles(candlesCount, (int)timeFrame));

        // ListenForCandles method must be ready before FetchCandles finishes execution.(_isListeningForCandles must become true)
        await Task.Delay(3000);

        await FetchHistoricalCandles((int)timeFrame, candlesCount);
        await FetchRecentCandles(candlesCount, (int)timeFrame);

        if (!_isListeningForCandles)
            throw new BingxException("System is not listening for new candles.");

        if (_candles.Count() < candlesCount - 3)
            throw new BingxException("System failed to fetch enough candles.");

        _areCandlesFetched = true;

        _logger.Information("Finished Candle Store initiation.");
    }

    private async Task FetchHistoricalCandles(int timeFrame, int candlesCount = 10000)
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

            if (!bingxCandles.Any() || (candles.Any() && candles.First().Date <= DateTimeOffset.FromUnixTimeMilliseconds(bingxCandles.Last().Time).DateTime))
                throw new BingxException("Server does not provide new candles.");

            if (candles.Any())
                for (int i = 0; i < bingxCandles.Count(); i++)
                    if (DateTimeOffset.FromUnixTimeMilliseconds(bingxCandles.ElementAt(i).Time).DateTime == candles.First().Date)
                    {
                        bingxCandles = bingxCandles.Skip(i + 1);
                        break;
                    }

            if ((candles.Count + bingxCandles.Count()) > candlesCount)
                bingxCandles = bingxCandles.Take(candlesCount - candles.Count);

            endTime = bingxCandles.Last().Time;

            List<Candle> convertedCandles = bingxCandles.Reverse().ToList().ConvertAll(o => new Candle()
            {
                Open = decimal.Parse(o.Open),
                Close = decimal.Parse(o.Close),
                High = decimal.Parse(o.High),
                Low = decimal.Parse(o.Low),
                Volume = decimal.Parse(o.Volume),
                Date = DateTimeOffset.FromUnixTimeMilliseconds(o.Time).DateTime
            });

            convertedCandles.AddRange(candles);
            candles = convertedCandles;
            _logger.Information($"{nameof(candles)}: {{candlesCount}}", candles.Count);
        }

        _areCandlesLocked = true;
        _candles = new Candles(candles);
        _candles.SkipLast(2);

        File.WriteAllText($"/home/hirbod/projects/bingx_ut_bot/src/bot/{Symbol}_{candles.Count}_HistoricalCandles_{GetStringTimeFrame(timeFrame)}.json", JsonSerializer.Serialize(_candles, new JsonSerializerOptions() { WriteIndented = true }));

        _areCandlesLocked = false;

        _logger.Information("Finished fetching historical candles...");
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

    public async Task FetchRecentCandles(int candlesCount, int timeFrame)
    {
        _logger.Information("Fetching recent candles...");
        _logger.Information($"{nameof(candlesCount)}: {{candlesCount}}", candlesCount);

        long startTime = DateTimeOffset.Parse(_candles.Last().Date.ToString()).ToUnixTimeMilliseconds();
        DateTime now = DateTime.UtcNow;
        now = now.AddSeconds(-1 * now.Second);
        while (Math.Floor((now.AddMinutes(-1) - _candles.Last().Date).TotalSeconds) >= _brokerOptions.TimeFrame)
        {
            IEnumerable<BingxCandle> bingxCandles = await GetKline(GetStringTimeFrame(timeFrame), Math.Min(1400, candlesCount), startTime);

            if (!bingxCandles.Any() || (_candles.Any() && _candles.Last().Date >= DateTimeOffset.FromUnixTimeMilliseconds(bingxCandles.First().Time).DateTime))
                throw new BingxException("Server does not provide new candles.");

            if (_candles.Any() && _candles.Last().Date == DateTimeOffset.FromUnixTimeMilliseconds(bingxCandles.Last().Time).DateTime)
                bingxCandles = bingxCandles.SkipLast(1);
            else
                for (int i = 0; i < bingxCandles.Count(); i++)
                    if (DateTimeOffset.FromUnixTimeMilliseconds(bingxCandles.ElementAt(i).Time).DateTime == _candles.Last().Date)
                    {
                        bingxCandles = bingxCandles.Take(i);
                        break;
                    }

            if (Math.Floor((DateTime.UtcNow - DateTimeOffset.FromUnixTimeMilliseconds(bingxCandles.First().Time).DateTime).TotalSeconds) < _brokerOptions.TimeFrame)
                bingxCandles = bingxCandles.Skip(1);

            startTime = bingxCandles.First().Time;

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
                              Date = DateTimeOffset.FromUnixTimeMilliseconds(o.Time).DateTime
                          });

            _areCandlesLocked = true;
            for (int i = 0; i < convertedCandles.Count; i++)
                _candles.Add(convertedCandles.ElementAt(i));
            _areCandlesLocked = false;

            now = DateTime.UtcNow;

            if (now.Second >= 30)
                await Task.Delay((60 - now.Second + 1) * 1000);
            now = now.AddSeconds(-1 * now.Second);
        }

        _logger.Information("Finished fetching recent candles...");
    }

    public async Task<Candles> GetCandles(int? timeFrameSeconds = null)
    {
        timeFrameSeconds ??= _brokerOptions.TimeFrame;

        while (true)
        {
            if (_areCandlesLocked)
                continue;

            if (!_candles.Any())
                await InitiateCandleStore(timeFrame: timeFrameSeconds);

            return _candles;
        }
    }

    public async Task<Candle> GetCandle(int indexFromEnd = 0)
    {
        while (true)
        {
            if (_areCandlesLocked)
                continue;

            Candle candle = _candles.ElementAt((await GetCandles()).Count() - indexFromEnd - 1);

            return candle;
        }
    }

    public Task<int> GetLastCandleIndex()
    {
        while (true)
        {
            if (_areCandlesLocked)
                continue;

            return Task.FromResult(_candles.Count() - 1);
        }
    }

    public async Task ListenForCandles(int candlesCount, int timeFrame)
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

        byte[] buffer = new byte[1024 * 4];
        Candle previousCandle = null!;
        while (true)
        {
            if (ws.State == WebSocketState.None || ws.State == WebSocketState.Connecting)
                continue;

            if (ws.State == WebSocketState.Closed || ws.State == WebSocketState.Aborted || ws.State == WebSocketState.CloseReceived)
            {
                string state = ws.State switch
                {
                    WebSocketState.CloseReceived => "CloseReceived",
                    WebSocketState.Closed => "Closed",
                    WebSocketState.Aborted => "Aborted",
                    _ => throw new BingxException("!")
                };
                _logger.Information($"Websocket state: {{{nameof(state)}}}", state);
                break;
            }

            WebSocketReceiveResult result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            _isListeningForCandles = true;

            if (result.MessageType == WebSocketMessageType.Close)
                break;

            byte[] bytes = await _utilities.DecompressBytes(buffer);

            string message = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            if (message == "Ping")
            {
                await ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("Pong")), WebSocketMessageType.Binary, true, CancellationToken.None);
                buffer = new byte[1024 * 4];
                continue;
            }

            BingxWsResponse? wsResponse = JsonSerializer.Deserialize<BingxWsResponse>(message, new JsonSerializerOptions(JsonSerializerDefaults.Web));

            if (wsResponse != null && wsResponse.Id != null && wsResponse.Id != id)
                throw new BingxException("Invalid id provided by the server.");

            if (wsResponse == null || wsResponse.Data == null || !wsResponse.Data.Any())
                continue;

            if (_candles == null)
                _candles = new(new Candle[]{ new()
                    {
                        Open = decimal.Parse(wsResponse.Data!.First().o),
                        Close = decimal.Parse(wsResponse.Data!.First().c),
                        High = decimal.Parse(wsResponse.Data!.First().h),
                        Low = decimal.Parse(wsResponse.Data!.First().l),
                        Date = DateTimeOffset.FromUnixTimeMilliseconds(wsResponse.Data!.First().T).DateTime,
                        Volume = decimal.Parse(wsResponse.Data!.First().v)
                    }}
                );
            else if (_candles.Any() && _candles.Last().Date == DateTimeOffset.FromUnixTimeMilliseconds(wsResponse.Data.First().T).DateTime)
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
                previousCandle = candle;
                continue;
            }

            if (previousCandle.Date == candle.Date)
            {
                previousCandle = candle;
                continue;
            }

            if (previousCandle.Date.AddSeconds(-1 * _brokerOptions.TimeFrame) != _candles.Last().Date)
            {
                _logger.Information("Missing candles detected!");

                previousCandle = candle;

                _areCandlesFetched = false;

                RunConcurrently(Task.Run(async () =>
                {
                    await FetchRecentCandles(candlesCount, timeFrame);
                    _areCandlesFetched = true;
                }));

                continue;
            }

            _areCandlesLocked = true;

            if (previousCandle.Date != _candles.Last().Date)
            {
                _candles.Add(previousCandle);
                _logger.Information("new candle added: {@candle}", candle);
                _logger.Information("Candles count: {count}", _candles.Count());
            }

            if (candlesCount < _candles.Count())
            {
                _candles.Skip(_candles.Count() - candlesCount);
            }

            _areCandlesLocked = false;

            previousCandle = candle;
        }

        await ws.CloseAsync(WebSocketCloseStatus.Empty, null, CancellationToken.None);

        _logger.Information("Finished listening for candles...");
    }

    private static void RunConcurrently(Task task)
    {
        if (task.Status == TaskStatus.Created)
            task.Start();
    }

    public Task<IEnumerable<Position?>> GetClosedPositions(DateTime start, DateTime? end = null)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Position?>> GetOpenPositions()
    {
        _logger.Information("Getting all the open positions...");

        HttpResponseMessage httpResponseMessage = await _utilities.HandleBingxRequest("https", Base_Url, "/openApi/swap/v2/trade/allOrders", "GET", ApiKey, ApiSecret, new
        {
            symbol = Symbol,
            startTime = DateTimeOffset.Parse(DateTime.UtcNow.AddHours(-5).ToString()).ToUnixTimeMilliseconds()
        });

        if (!await _utilities.TryEnsureSuccessfulBingxResponse(httpResponseMessage))
            throw new CloseAllPositionsException();

        BingxResponse<IEnumerable<BingxPositionDto>> bingxResponse = JsonSerializer.Deserialize<BingxResponse<IEnumerable<BingxPositionDto>>>(await httpResponseMessage.Content.ReadAsStringAsync(), new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new CloseAllPositionsException();

        _logger.Information("Finished getting all the open positions...");
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

    public Task ClosePosition(Position position)
    {
        throw new NotImplementedException();
    }
}
