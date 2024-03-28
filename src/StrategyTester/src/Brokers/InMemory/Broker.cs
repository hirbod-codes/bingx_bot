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

    public Broker(IBrokerOptions brokerOptions, IPositionRepository positionRepository, ITime time, ILogger logger)
    {
        _time = time;
        _logger = logger.ForContext<Broker>();
        _brokerOptions = (brokerOptions as BrokerOptions)!;
        _positionRepository = positionRepository;
    }

    public Task InitiateCandleStore(int? candlesCount = null, int? timeFrame = null)
    {
        _logger.Information("Initiating Candle Store...");

        timeFrame ??= 60;
        string timeFrameString = null!;

        if (timeFrame == 60)
            timeFrameString = "1m";

        if (timeFrame == (60 * 15))
            timeFrameString = "15m";

        if (timeFrame == (60 * 60))
            timeFrameString = "1h";

        IEnumerable<Candle> candles = JsonSerializer.Deserialize<IEnumerable<Candle>>(File.ReadAllText($"/home/hirbod/projects/bingx_ut_bot/src/bot/{_brokerOptions.Symbol}_HistoricalCandles_{timeFrameString}.json"), new JsonSerializerOptions(JsonSerializerDefaults.Web))!;

        if (candlesCount != null)
            candles = candles.Take((int)candlesCount);
        // candles = candles.Where(c => c.Date >= DateTime.Parse("2023-12-15T10:20:00"));

        _candles = new(candles.ToList());

        _candlesCount = candles.Count();

        _passedCandlesIndex = 0;

        _logger.Information("Finished candle store initiation.");
        _logger.Information("Candle store count: {count}", _candlesCount);

        return Task.CompletedTask;
    }

    public void NextCandle()
    {
        _passedCandlesIndex++;
        _logger.Information("Number passed candles: {count}", _passedCandlesIndex + 1);
    }

    public bool IsFinished() => _passedCandlesIndex + 1 == _candlesCount;

    public async Task<Candles> GetCandles(int? timeFrameSeconds = null)
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

        Candle candle = await GetCandle();

        if (await _positionRepository.AnyPendingPosition())
        {
            Position?[] pendingPositions = (await _positionRepository.GetPendingPositions()).ToArray();

            long[] ticksArray = Array.Empty<long>();
            for (int i = 0; i < pendingPositions.Length; i++)
            {
                if (pendingPositions[i] == null)
                    continue;

                if (
                    (pendingPositions[i]!.PositionDirection == PositionDirection.SHORT && candle.Low > pendingPositions[i]!.OpenedPrice)
                    ||
                    (pendingPositions[i]!.PositionDirection == PositionDirection.LONG && candle.High < pendingPositions[i]!.OpenedPrice)
                )
                    continue;

                if (ticksArray.Contains(((DateTime)pendingPositions[i]!.CreatedAt!).Ticks))
                    continue;

                ticksArray = ticksArray.Append(_time.GetUtcNow().Ticks).ToArray();

                await _positionRepository.OpenPosition(pendingPositions[i]!.Id, candle.Date.AddSeconds(_brokerOptions.TimeFrame));
            }
        }

        if (await _positionRepository.AnyOpenedPosition())
        {
            Position?[] openPositions = (await _positionRepository.GetOpenedPositions()).ToArray();

            for (int i = 0; i < openPositions.Length; i++)
            {
                if (openPositions[i] == null)
                    continue;

                if (
                    (openPositions[i]!.PositionDirection == PositionDirection.LONG && candle.Low <= openPositions[i]!.SLPrice)
                    ||
                    (openPositions[i]!.PositionDirection == PositionDirection.SHORT && candle.High >= openPositions[i]!.SLPrice)
                )
                {
                    await ClosePosition(openPositions[i]!);
                    continue;
                }

                if (openPositions[i]!.TPPrice == null)
                    continue;

                if (
                    (openPositions[i]!.PositionDirection == PositionDirection.LONG && candle.High >= openPositions[i]!.TPPrice)
                    ||
                    (openPositions[i]!.PositionDirection == PositionDirection.SHORT && candle.Low <= openPositions[i]!.TPPrice)
                )
                    await ClosePosition(openPositions[i]!);
            }
        }
    }

    public async Task ClosePosition(Position position)
    {
        Candle candle = await GetCandle();
        bool unknownState = false;
        decimal? closedPrice = null!;
        if (position.PositionDirection == PositionDirection.LONG)
        {
            if (candle.Low <= position.SLPrice && position.TPPrice != null && candle.High >= position.TPPrice)
            {
                unknownState = true;
                closedPrice = position.SLPrice;
            }
            else if (candle.Low <= position.SLPrice)
                closedPrice = position.SLPrice;
            else if (position.TPPrice != null && candle.High >= position.TPPrice)
                closedPrice = (decimal)position.TPPrice;
            else if (position.TPPrice != null)
                throw new ClosePositionException();
        }
        else if (position.PositionDirection == PositionDirection.SHORT)
        {
            if (candle.High >= position.SLPrice && position.TPPrice != null && candle.Low <= position.TPPrice)
            {
                unknownState = true;
                closedPrice = position.SLPrice;
            }
            else if (candle.High >= position.SLPrice)
                closedPrice = position.SLPrice;
            else if (position.TPPrice != null && candle.Low <= position.TPPrice)
                closedPrice = (decimal)position.TPPrice;
            else if (position.TPPrice != null)
                throw new ClosePositionException();
        }
        else
            throw new ClosePositionException();

        await ClosePosition(position.Id, (decimal)closedPrice, candle.Date.AddSeconds(_brokerOptions.TimeFrame), unknownState);
    }

    public async Task ClosePosition(string id, decimal closedPrice, DateTime closedAt, bool unknownState) => await _positionRepository.ClosePosition(id, closedPrice, closedAt, _brokerOptions.BrokerCommission, unknownState);

    public async Task CancelPosition(string id, DateTime cancelledAt) => await _positionRepository.CancelPosition(id, cancelledAt);

    public async Task CloseAllPositions()
    {
        Candle candle = await GetCandle();
        await CloseAllPositions(candle.Close, candle.Date.AddSeconds(_brokerOptions.TimeFrame));
    }

    public async Task CloseAllPositions(decimal closedPrice, DateTime closedAt)
    {
        Position?[] positions = (await GetOpenPositions()).ToArray();

        for (int i = 0; i < positions.Length; i++)
            if (positions[i] == null)
                continue;
            else
                await ClosePosition(positions[i]!.Id, closedPrice, closedAt, false);
    }

    public async Task CancelAllPendingPositions()
    {
        Position?[] positions = (await GetPendingPositions()).ToArray();
        await CancelAllPendingPositions(positions);
    }

    public async Task<IEnumerable<Position?>> GetOpenPositions() => await _positionRepository.GetOpenedPositions();

    public Task<IEnumerable<Position?>> GetClosedPositions(DateTime start, DateTime? end = null) => _positionRepository.GetClosedPositions(start, end);

    public async Task<IEnumerable<Position?>> GetPendingPositions() => await _positionRepository.GetPendingPositions();

    public async Task OpenMarketPosition(decimal margin, decimal leverage, string direction, decimal slPrice, decimal tpPrice)
    {
        if (direction != PositionDirection.LONG && direction != PositionDirection.SHORT)
            throw new BrokerException();

        Candle candle = await GetCandle();
        await _positionRepository.CreateOpenPosition(new Position()
        {
            Leverage = leverage,
            Margin = margin,
            CreatedAt = candle.Date.AddSeconds(_brokerOptions.TimeFrame),
            OpenedAt = candle.Date.AddSeconds(_brokerOptions.TimeFrame),
            OpenedPrice = await GetLastPrice(),
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

        Candle candle = await GetCandle();
        await _positionRepository.CreateOpenPosition(new Position()
        {
            Leverage = leverage,
            Margin = margin,
            CreatedAt = candle.Date.AddSeconds(_brokerOptions.TimeFrame),
            OpenedAt = candle.Date.AddSeconds(_brokerOptions.TimeFrame),
            OpenedPrice = await GetLastPrice(),
            SLPrice = slPrice,
            CommissionRatio = _brokerOptions.BrokerCommission,
            Symbol = _brokerOptions.Symbol,
            PositionDirection = direction
        });
    }

    public async Task OpenLimitPosition(decimal margin, decimal leverage, string direction, decimal limit, decimal slPrice)
    {
        if (direction != PositionDirection.LONG && direction != PositionDirection.SHORT)
            throw new BrokerException();

        Candle candle = await GetCandle();
        await _positionRepository.CreatePendingPosition(new Position()
        {
            Leverage = leverage,
            Margin = margin,
            CreatedAt = candle.Date.AddSeconds(_brokerOptions.TimeFrame),
            OpenedPrice = limit,
            SLPrice = slPrice,
            CommissionRatio = _brokerOptions.BrokerCommission,
            Symbol = _brokerOptions.Symbol,
            PositionDirection = direction
        });
    }

    public async Task OpenLimitPosition(decimal margin, decimal leverage, string direction, decimal limit, decimal slPrice, decimal tpPrice)
    {
        if (direction != PositionDirection.LONG && direction != PositionDirection.SHORT)
            throw new BrokerException();

        Candle candle = await GetCandle();
        await _positionRepository.CreatePendingPosition(new Position()
        {
            Leverage = leverage,
            Margin = margin,
            CreatedAt = candle.Date.AddSeconds(_brokerOptions.TimeFrame),
            OpenedPrice = limit,
            SLPrice = slPrice,
            TPPrice = tpPrice,
            CommissionRatio = _brokerOptions.BrokerCommission,
            Symbol = _brokerOptions.Symbol,
            PositionDirection = direction
        });
    }

    private async Task CancelAllPendingPositions(Position?[] positions)
    {
        for (int i = 0; i < positions.Length; i++)
            if (positions[i] == null)
                continue;
            else
                await CancelPosition(positions[i]!.Id, _time.GetUtcNow());
    }

    public async Task CancelAllLongPendingPositions()
    {
        Position?[] positions = (await GetPendingPositions()).Where(o => o != null && o.PositionDirection == PositionDirection.LONG).ToArray();
        await CancelAllPendingPositions(positions);
    }

    public async Task CancelAllShortPendingPositions()
    {
        Position?[] positions = (await GetPendingPositions()).Where(o => o != null && o.PositionDirection == PositionDirection.SHORT).ToArray();
        await CancelAllPendingPositions(positions);
    }
}
