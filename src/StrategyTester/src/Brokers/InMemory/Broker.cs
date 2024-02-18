using System.Text.Json;
using StrategyTester.src.Brokers.InMemory.Exceptions;
using bot.src.Data;
using bot.src.Data.Models;
using Serilog;
using bot.src.Brokers;
using StrategyTester.src.Utils;
using BrokerException = StrategyTester.src.Brokers.InMemory.Exceptions.BrokerException;

namespace StrategyTester.src.Brokers.InMemory;

public class Broker : IBroker
{
    private readonly IPositionRepository _positionRepository;
    private readonly ITime _time;
    private readonly ILogger _logger;
    private readonly BrokerOptions _brokerOptions;

    private Candles _candles = null!;
    private int _candlesCount = 0;
    private int _passedCandlesIndex = 0;

    private decimal? _positionsUpperBand = null;
    private decimal? _positionsLowerBand = null;

    public Broker(IBrokerOptions brokerOptions, IPositionRepository positionRepository, ITime time, ILogger logger)
    {
        _time = time;
        _logger = logger.ForContext<Broker>();
        _brokerOptions = (brokerOptions as BrokerOptions)!;
        _positionRepository = positionRepository;
    }

    public Task InitiateCandleStore(int candlesCount = 10000)
    {
        _logger.Information("Initiating Candle Store...");

        IEnumerable<Candle> candles = JsonSerializer.Deserialize<IEnumerable<Candle>>(File.ReadAllText("/home/hirbod/projects/bingx_ut_bot/src/bot/HistoricalCandles.json"), new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        // IEnumerable<Candle> candles = JsonSerializer.Deserialize<IEnumerable<Candle>>(File.ReadAllText("/home/hirbod/projects/bingx_ut_bot/src/StrategyTester/Brokers/fetched_data/twelvedata.json"), new JsonSerializerOptions(JsonSerializerDefaults.Web))!.ToList();
        // IEnumerable<Candle> candles = JsonSerializer.Deserialize<IEnumerable<Candle>>(File.ReadAllText("/home/hirbod/projects/bingx_ut_bot/src/StrategyTester/Brokers/fetched_data/kline_raw_data_Y-1-18__12:37:36.json"), new JsonSerializerOptions(JsonSerializerDefaults.Web))!.ToList();
        // IEnumerable<Candle> candles = JsonSerializer.Deserialize<IEnumerable<Candle>>(File.ReadAllText("/home/hirbod/projects/bingx_ut_bot/src/StrategyTester/Brokers/fetched_data/kline_data_one_month_1min.json"), new JsonSerializerOptions(JsonSerializerDefaults.Web))!.ToList();
        // IEnumerable<Candle> candles = JsonSerializer.Deserialize<IEnumerable<Candle>>(File.ReadAllText("/home/hirbod/projects/bingx_ut_bot/src/StrategyTester/Brokers/fetched_data/3month_kline_data.json"), new JsonSerializerOptions(JsonSerializerDefaults.Web))!.ToList();

        _candles = new(candles.ToList());

        _candlesCount = candles.Count();

        _passedCandlesIndex = 0;

        _logger.Information("Finished Candle Store initiation.");

        return Task.CompletedTask;
    }

    public void NextCandle()
    {
        _passedCandlesIndex++;
        _logger.Information("Number passed candles: {count}", _passedCandlesIndex + 1);
    }

    public bool IsFinished() => _passedCandlesIndex + 1 == _candlesCount;

    public async Task<Candles> GetCandles()
    {
        if (!_candles.Any())
            await InitiateCandleStore();
        return _candles;
    }

    public Task<Candle> GetCandle(int indexFromEnd = 0) => Task.FromResult(_candles.ElementAt(_passedCandlesIndex - indexFromEnd));

    public Task<int> GetLastCandleIndex() => Task.FromResult(_passedCandlesIndex);

    public async Task<decimal> GetLastPrice() => (await GetCandle()).Close;

    public async Task CandleClosed()
    {
        _logger.Information("Getting open positions...");

        if (_positionsUpperBand == null && _positionsLowerBand == null)
            return;

        Candle candle = await GetCandle();

        if (_positionsLowerBand == null && candle.High < _positionsUpperBand)
            return;

        if (_positionsUpperBand == null && candle.Low > _positionsLowerBand)
            return;

        if (_positionsUpperBand != null && _positionsLowerBand != null && candle.Low > _positionsLowerBand && candle.High < _positionsUpperBand)
            return;

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

        await SetPositionsLowerUpperBand();
    }

    private async Task SetPositionsLowerUpperBand()
    {
        IEnumerable<Position> openPositions = await _positionRepository.GetOpenedPositions();

        _positionsUpperBand = null;
        _positionsLowerBand = null;

        if (!openPositions.Any())
            return;

        foreach (Position position in openPositions)
        {
            if (position.PositionDirection == PositionDirection.LONG && position.SLPrice > _positionsLowerBand)
                _positionsLowerBand = position.SLPrice;
            if (position.PositionDirection == PositionDirection.SHORT && position.SLPrice < _positionsUpperBand)
                _positionsUpperBand = position.SLPrice;

            if (position.TPPrice == null)
                continue;

            if (position.PositionDirection == PositionDirection.LONG && position.TPPrice < _positionsUpperBand)
                _positionsUpperBand = position.TPPrice;
            if (position.PositionDirection == PositionDirection.SHORT && position.TPPrice > _positionsLowerBand)
                _positionsLowerBand = position.TPPrice;
        }
    }

    private async Task<bool> ShouldClosePosition(Position position)
    {
        Candle candle = await GetCandle();
        return (
            position.PositionDirection == PositionDirection.LONG &&
            (candle.Low <= position.SLPrice || (position.TPPrice != null && candle.High >= position.TPPrice))
        ) ||
        (
            position.PositionDirection == PositionDirection.SHORT &&
            (candle.High >= position.SLPrice || (position.TPPrice != null && candle.Low <= position.TPPrice))
        );
    }

    public async Task ClosePosition(Position position)
    {
        Candle candle = await GetCandle();
        decimal? closedPrice = null!;
        if (position.PositionDirection == PositionDirection.LONG)
        {
            if (candle.Low <= position.SLPrice)
                closedPrice = position.SLPrice;
            else if (position.TPPrice != null && candle.High >= position.TPPrice)
                closedPrice = (decimal)position.TPPrice;
        }
        else if (position.PositionDirection == PositionDirection.SHORT)
        {
            if (candle.High >= position.SLPrice)
                closedPrice = position.SLPrice;
            else if (position.TPPrice != null && candle.Low <= position.TPPrice)
                closedPrice = (decimal)position.TPPrice;
        }
        else
            throw new ClosePositionException();

        await ClosePosition(position.Id, (decimal)closedPrice, candle.Date.AddSeconds(_brokerOptions.TimeFrame));

        await SetPositionsLowerUpperBand();
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

    public async Task CloseAllPositions()
    {
        Candle candle = await GetCandle();
        await CloseAllPositions(candle.Close, candle.Date.AddSeconds(_brokerOptions.TimeFrame));

        await SetPositionsLowerUpperBand();
    }

    public async Task CloseAllPositions(decimal closedPrice, DateTime closedAt)
    {
        IEnumerable<Position> openPositions = await GetOpenPositions();

        foreach (Position position in openPositions)
            await ClosePosition(position.Id, closedPrice, closedAt);
    }

    public async Task<IEnumerable<Position>> GetOpenPositions() => await _positionRepository.GetOpenedPositions();

    public Task<IEnumerable<Position>> GetClosedPositions(DateTime start, DateTime? end = null) => _positionRepository.GetClosedPositions(start, end);

    public async Task OpenMarketPosition(decimal margin, decimal leverage, string direction, decimal slPrice, decimal tpPrice)
    {
        if (direction != PositionDirection.LONG && direction != PositionDirection.SHORT)
            throw new BrokerException();

        if (direction == PositionDirection.LONG && (_positionsLowerBand == null || slPrice > _positionsLowerBand))
            _positionsLowerBand = slPrice;
        if (direction == PositionDirection.LONG && (_positionsUpperBand == null || tpPrice < _positionsUpperBand))
            _positionsUpperBand = tpPrice;

        if (direction == PositionDirection.SHORT && (_positionsUpperBand == null || slPrice < _positionsUpperBand))
            _positionsUpperBand = slPrice;
        if (direction == PositionDirection.SHORT && (_positionsLowerBand == null || tpPrice > _positionsLowerBand))
            _positionsLowerBand = tpPrice;

        Candle candle = await GetCandle();
        await _positionRepository.CreatePosition(new Position()
        {
            Leverage = leverage,
            Margin = margin,
            OpenedAt = candle.Date.AddSeconds(_brokerOptions.TimeFrame),
            OpenedPrice = candle.Close,
            SLPrice = slPrice,
            TPPrice = tpPrice,
            CommissionRatio = _brokerOptions.BrokerCommission,
            Symbol = _brokerOptions.Symbol,
            PositionDirection = direction,
        });
    }

    public async Task OpenMarketPosition(decimal margin, decimal leverage, string direction, decimal slPrice)
    {
        if (direction != PositionDirection.LONG && direction != PositionDirection.SHORT)
            throw new BrokerException();

        if (direction == PositionDirection.LONG && (_positionsLowerBand == null || slPrice > _positionsLowerBand))
            _positionsLowerBand = slPrice;

        if (direction == PositionDirection.SHORT && (_positionsUpperBand == null || slPrice < _positionsUpperBand))
            _positionsUpperBand = slPrice;

        Candle candle = await GetCandle();
        await _positionRepository.CreatePosition(new Position()
        {
            Leverage = leverage,
            Margin = margin,
            OpenedAt = candle.Date.AddSeconds(_brokerOptions.TimeFrame),
            OpenedPrice = candle.Close,
            SLPrice = slPrice,
            CommissionRatio = _brokerOptions.BrokerCommission,
            Symbol = _brokerOptions.Symbol,
            PositionDirection = direction,
        });
    }
}
