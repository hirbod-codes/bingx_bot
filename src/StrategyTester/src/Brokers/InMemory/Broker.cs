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

    // private List<(string id, decimal price)> _positionsUpperBand = new();
    // private List<(string id, decimal price)> _positionsLowerBand = new();

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

        candles = candles.Take(10000);
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

        if (!await _positionRepository.AnyOpenedPosition())
            return;

        Position?[] openPositions = (await _positionRepository.GetOpenedPositions()).ToArray();

        Candle candle = await GetCandle();

        int i = 0;
        for (; i < openPositions.Length; i++)
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

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        // if (_positionsUpperBand.Count == 0 && _positionsLowerBand.Count == 0)
        //     return;

        // Candle candle = await GetCandle();

        // if (_positionsLowerBand.Count == 0 && candle.High < _positionsUpperBand[0].price)
        //     return;

        // if (_positionsUpperBand.Count == 0 && candle.Low > _positionsLowerBand[0].price)
        //     return;

        // if (_positionsUpperBand.Count != 0 && _positionsLowerBand.Count != 0 && candle.Low > _positionsLowerBand[0].price && candle.High < _positionsUpperBand[0].price)
        //     return;

        // _logger.Information("Closing open positions that are suppose to be closed.");

        // int closedPositionsCount = 0;
        // if (_positionsUpperBand.Count != 0)
        // {
        //     int i = 0;
        //     if (candle.High >= _positionsUpperBand.Last().price)
        //         i = _positionsUpperBand.Count;
        //     else
        //         for (; i < _positionsUpperBand.Count; i++)
        //             if (candle.High < _positionsUpperBand[i].price)
        //                 break;

        //     for (int j = 0; j < i; j++)
        //     {
        //         await ClosePosition((await _positionRepository.GetOpenedPosition(_positionsUpperBand[j].id))!);
        //         closedPositionsCount++;

        //         _positionsLowerBand = _positionsLowerBand.Where(o => o.id != _positionsUpperBand[j].id).ToList();
        //     }

        //     _positionsUpperBand = _positionsUpperBand.Skip(i).ToList();
        // }

        // if (_positionsLowerBand.Count != 0)
        // {
        //     int i = 0;
        //     if (candle.Low <= _positionsLowerBand.Last().price)
        //         i = _positionsLowerBand.Count;
        //     else
        //         for (; i < _positionsLowerBand.Count; i++)
        //             if (candle.Low > _positionsLowerBand[i].price)
        //                 break;

        //     for (int j = 0; j < i; j++)
        //     {
        //         await ClosePosition((await _positionRepository.GetOpenedPosition(_positionsLowerBand[j].id))!);
        //         closedPositionsCount++;

        //         _positionsUpperBand = _positionsUpperBand.Where(o => o.id != _positionsLowerBand[j].id).ToList();
        //     }

        //     _positionsLowerBand = _positionsLowerBand.Skip(i).ToList();
        // }

        // _logger.Information("{closedPositionsCount} positions closed.", closedPositionsCount);
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
    }

    public async Task ClosePosition(string id, decimal closedPrice, DateTime closedAt) => await _positionRepository.ClosePosition(id, closedPrice, closedAt, _brokerOptions.BrokerCommission);

    public async Task CloseAllPositions()
    {
        Candle candle = await GetCandle();
        await CloseAllPositions(candle.Close, candle.Date.AddSeconds(_brokerOptions.TimeFrame));

        // _positionsLowerBand = new();
        // _positionsUpperBand = new();
    }

    public async Task CloseAllPositions(decimal closedPrice, DateTime closedAt)
    {
        Position?[] positions = (await GetOpenPositions()).ToArray();

        for (int i = 0; i < positions.Length; i++)
            if (positions[i] == null)
                continue;
            else
                await ClosePosition(positions[i]!.Id, closedPrice, closedAt);
    }

    public async Task<IEnumerable<Position?>> GetOpenPositions() => await _positionRepository.GetOpenedPositions();

    public Task<IEnumerable<Position?>> GetClosedPositions(DateTime start, DateTime? end = null) => _positionRepository.GetClosedPositions(start, end);

    public async Task OpenMarketPosition(decimal margin, decimal leverage, string direction, decimal slPrice, decimal tpPrice)
    {
        if (direction != PositionDirection.LONG && direction != PositionDirection.SHORT)
            throw new BrokerException();

        Candle candle = await GetCandle();
        Position position = await _positionRepository.CreatePosition(new Position()
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

        AddSlPrice(position.Id, slPrice, direction);
        AddTpPrice(position.Id, tpPrice, direction);
    }

    public async Task OpenMarketPosition(decimal margin, decimal leverage, string direction, decimal slPrice)
    {
        if (direction != PositionDirection.LONG && direction != PositionDirection.SHORT)
            throw new BrokerException();

        Candle candle = await GetCandle();
        Position position = await _positionRepository.CreatePosition(new Position()
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

        AddSlPrice(position.Id, slPrice, direction);
    }

    private void AddSlPrice(string id, decimal slPrice, string direction)
    {
        // if (direction == PositionDirection.LONG)
        // {
        //     if (_positionsLowerBand.Count == 0)
        //         _positionsLowerBand.Add((id, slPrice));
        //     else if (slPrice <= _positionsLowerBand.Last().price)
        //         _positionsLowerBand.Add((id, slPrice));
        //     else
        //         _positionsLowerBand.Insert(_positionsLowerBand.FindIndex(t => slPrice > t.price), (id, slPrice));
        // }
        // else if (direction == PositionDirection.SHORT)
        // {
        //     if (_positionsUpperBand.Count == 0)
        //         _positionsUpperBand.Add((id, slPrice));
        //     else if (slPrice >= _positionsUpperBand.Last().price)
        //         _positionsUpperBand.Add((id, slPrice));
        //     else
        //         _positionsUpperBand.Insert(_positionsUpperBand.FindIndex(t => slPrice < t.price), (id, slPrice));
        // }
        // else
        //     throw new BrokerException("Invalid direction detected for a position");
    }

    private void AddTpPrice(string id, decimal tpPrice, string direction)
    {
        // if (direction == PositionDirection.SHORT)
        // {
        //     if (_positionsLowerBand.Count == 0)
        //         _positionsLowerBand.Add((id, tpPrice));
        //     else if (tpPrice <= _positionsLowerBand.Last().price)
        //         _positionsLowerBand.Add((id, tpPrice));
        //     else
        //         _positionsLowerBand.Insert(_positionsLowerBand.FindIndex(t => tpPrice > t.price), (id, tpPrice));
        // }
        // else if (direction == PositionDirection.LONG)
        // {
        //     if (_positionsUpperBand.Count == 0)
        //         _positionsUpperBand.Add((id, tpPrice));
        //     else if (tpPrice >= _positionsUpperBand.Last().price)
        //         _positionsUpperBand.Add((id, tpPrice));
        //     else
        //         _positionsUpperBand.Insert(_positionsUpperBand.FindIndex(t => tpPrice < t.price), (id, tpPrice));
        // }
        // else
        //     throw new BrokerException("Invalid direction detected for a position");
    }
}
