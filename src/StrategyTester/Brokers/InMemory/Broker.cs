using System.Text.Json;
using StrategyTester.src.Brokers.InMemory;
using StrategyTester.src.Brokers.InMemory.Exceptions;
using bot.src.Data;
using bot.src.Data.Models;
using Serilog;
using bot.src.Brokers;
using StrategyTester.src.Utils;

namespace StrategyTester.src.Brokers.InMemory;

public class Broker : IBroker
{
    private readonly IPositionRepository _positionRepository;
    private readonly ITime _time;
    private readonly ILogger _logger;
    private readonly BrokerOptions _brokerOptions;

    private Candles _passedCandles = new();
    private Candles _candles = null!;

    public Broker(IBrokerOptions brokerOptions, IPositionRepository positionRepository, ITime time, ILogger logger)
    {
        _time = time;
        _logger = logger.ForContext<Broker>();
        _brokerOptions = (brokerOptions as BrokerOptions)!;
        _positionRepository = positionRepository;
    }

    public Task InitiateCandleStore(int candlesCount = 10000)
    {
        _candles = new();

        IEnumerable<Candle> candles = JsonSerializer.Deserialize<IEnumerable<Candle>>(File.ReadAllText("/home/hirbod/projects/bingx_ut_bot/src/bot/HistoricalCandles.json"), new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new Exception("No data provider");
        // IEnumerable<Candle> candles = JsonSerializer.Deserialize<IEnumerable<Candle>>(File.ReadAllText("/home/hirbod/projects/bingx_ut_bot/src/StrategyTester/Brokers/fetched_data/twelvedata.json"), new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new Exception("No data provider");
        // IEnumerable<Candle> candles = JsonSerializer.Deserialize<IEnumerable<Candle>>(File.ReadAllText("/home/hirbod/projects/bingx_ut_bot/src/StrategyTester/Brokers/fetched_data/kline_raw_data_Y-1-18__12:37:36.json"), new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new Exception("No data provider");
        // IEnumerable<Candle> candles = JsonSerializer.Deserialize<IEnumerable<Candle>>(File.ReadAllText("/home/hirbod/projects/bingx_ut_bot/src/StrategyTester/Brokers/fetched_data/kline_data_one_month_1min.json"), new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new Exception("No data provider");
        // IEnumerable<Candle> candles = JsonSerializer.Deserialize<IEnumerable<Candle>>(File.ReadAllText("/home/hirbod/projects/bingx_ut_bot/src/StrategyTester/Brokers/fetched_data/3month_kline_data.json"), new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new Exception("No data provider");

        _candles.SetCandles(candles);

        _passedCandles.AddCandle(_candles.First());

        _time.SetUtcNow(_candles.First().Date);

        RunConcurrently(ListenForCandles());

        return Task.CompletedTask;
    }

    private async Task ListenForCandles()
    {
        while (_passedCandles.Count() == _candles.Count())
        {
            await Task.Delay(2);

            if (_passedCandles.Last().Date == _time.GetUtcNow())
                continue;

            Candle candle = _candles.ElementAt(_passedCandles.Count());
            _passedCandles.AddCandle(candle);
            _time.SetUtcNow(candle.Date);
        }
    }

    public void RunConcurrently(Task task)
    {
        if (task.Status == TaskStatus.Created)
            task.Start();
    }

    public async Task<Candles> GetCandles()
    {
        if (!_candles.Any())
            await InitiateCandleStore();
        return _passedCandles;
    }

    public Task<Candle> GetCandle(int indexFromEnd = 0) => Task.FromResult(_passedCandles.ElementAt(_passedCandles.Count() - 1 - indexFromEnd));

    public async Task<decimal> GetLastPrice() => (await GetCandle()).Close;

    public async Task CandleClosed()
    {
        _logger.Information("Getting open positions...");

        IEnumerable<Position> openPositions = await _positionRepository.GetOpenedPositions();

        if (!openPositions.Any())
        {
            _logger.Information("There is no open position.");
            return;
        }

        _logger.Information("Closing open positions that are suppose to be closed.");

        int closedPositionsCount = 0;
        foreach (Position position in openPositions)
            if (await ShouldClosePosition(position))
            {
                closedPositionsCount++;
                await ClosePosition(position);
            }
        _logger.Information("{closedPositionsCount} positions closed.", closedPositionsCount);
    }

    private async Task<bool> ShouldClosePosition(Position position) =>
        (
            position.PositionDirection == PositionDirection.LONG &&
            ((await GetCandle()).Low <= position.SLPrice || (position.TPPrice != null && (await GetCandle()).High >= position.TPPrice))
        ) ||
        (
            position.PositionDirection == PositionDirection.SHORT &&
            ((await GetCandle()).High >= position.SLPrice || (position.TPPrice != null && (await GetCandle()).Low <= position.TPPrice))
        );

    public async Task ClosePosition(Position position)
    {
        decimal? closedPrice = null!;
        if (position.PositionDirection == PositionDirection.LONG)
        {
            if ((await GetCandle()).Low <= position.SLPrice)
                closedPrice = position.SLPrice;
            else if (position.TPPrice != null && (await GetCandle()).High >= position.TPPrice)
                closedPrice = (decimal)position.TPPrice;
        }
        else if (position.PositionDirection == PositionDirection.SHORT)
        {
            if ((await GetCandle()).High >= position.SLPrice)
                closedPrice = position.SLPrice;
            else if (position.TPPrice != null && (await GetCandle()).Low <= position.TPPrice)
                closedPrice = (decimal)position.TPPrice;
        }
        else
            throw new ClosePositionException();

        await ClosePosition(position.Id, (decimal)closedPrice, (await GetCandle()).Date.AddSeconds(_candles.TimeFrame));
    }

    public async Task CloseAllPositions()
    {
        Candle candle = await GetCandle();
        await CloseAllPositions(candle.Close, candle.Date.AddSeconds(_candles.TimeFrame));
    }

    public async Task CloseAllPositions(decimal closedPrice, DateTime closedAt)
    {
        IEnumerable<Position> openPositions = await GetOpenPositions();

        foreach (Position position in openPositions)
            await ClosePosition(position.Id, closedPrice, closedAt);
    }

    public async Task ClosePosition(string id, decimal closedPrice, DateTime closedAt)
    {
        Position position = await _positionRepository.GetPosition(id) ?? throw new PositionNotFoundException();

        if (position.PositionStatus == PositionStatus.CLOSED)
            throw new ClosingAClosedPosition();

        position.ClosedPrice = closedPrice;
        position.ClosedAt = closedAt;

        decimal? profit = (position.ClosedPrice - position.OpenedPrice) * position.Margin * position.Leverage / position.OpenedPrice;
        if (position.PositionDirection == PositionDirection.SHORT)
            profit *= -1;
        position.Profit = profit;
        decimal commission = _brokerOptions.BrokerCommission * position.Margin * position.Leverage;
        position.Commission = commission;
        position.ProfitWithCommission = profit - commission;

        position.PositionStatus = PositionStatus.CLOSED;

        await _positionRepository.ReplacePosition(position);
    }

    public async Task<IEnumerable<Position>> GetOpenPositions() => await _positionRepository.GetOpenedPositions();

    public Task<IEnumerable<Position>> GetClosedPositions(DateTime start, DateTime? end = null) => _positionRepository.GetClosedPositions(start, end);

    public async Task OpenMarketPosition(decimal margin, decimal leverage, string direction, decimal slPrice, decimal tpPrice) => await _positionRepository.CreatePosition(new Position()
    {
        Leverage = leverage,
        Margin = margin,
        OpenedAt = (await GetCandle()).Date.AddSeconds(_candles.TimeFrame),
        OpenedPrice = (await GetCandle()).Close,
        SLPrice = slPrice,
        TPPrice = tpPrice,
        CommissionRatio = _brokerOptions.BrokerCommission,
        Symbol = _brokerOptions.Symbol,
        PositionDirection = direction,
    });

    public async Task OpenMarketPosition(decimal margin, decimal leverage, string direction, decimal slPrice) => await _positionRepository.CreatePosition(new Position()
    {
        Leverage = leverage,
        Margin = margin,
        OpenedAt = (await GetCandle()).Date.AddSeconds(_candles.TimeFrame),
        OpenedPrice = (await GetCandle()).Close,
        SLPrice = slPrice,
        CommissionRatio = _brokerOptions.BrokerCommission,
        Symbol = _brokerOptions.Symbol,
        PositionDirection = direction,
    });
}
