using Abstractions.src.Models;
using Abstractions.src.Repository;

namespace Repositories.src.InMemory;

public class CandleRepository : ICandleRepository
{
    private Dictionary<int, Candles?> _candles = new();

    public Task<Candles?> GetCandles(int timeFrame) => Task.FromResult(_candles[timeFrame]);
    public Task<Candles?> GetCandles(int timeFrame, int count) => Task.FromResult(_candles[timeFrame]?.TakeLast(count));
    public Task<Candle?> GetLastCandle(int timeFrame) => Task.FromResult(_candles[timeFrame]?.LastOrDefault());
    public Task<Candle?> GetCandle(int timeFrame, int index) => Task.FromResult(_candles[timeFrame]?.ElementAtOrDefault(index));
    public Task<Candle?> GetCandle(int timeFrame, DateTime openedAt) => Task.FromResult(_candles[timeFrame]?.FirstOrDefault(c => c.Date == openedAt));

    public Task AddCandle(int timeFrame, Candle candle)
    {
        Candles? candles = _candles.GetValueOrDefault(timeFrame);

        if (candles == null)
            _candles[timeFrame] = new(new Candle[] { candle });
        else
            candles.Add(candle);

        return Task.CompletedTask;
    }

    public Task AddCandles(int timeFrame,IEnumerable<Candle> candles)
    {
        Candles? dictionaryCandles = _candles.GetValueOrDefault(timeFrame);

        if (dictionaryCandles == null)
            _candles[timeFrame] = new(candles);
        else
            dictionaryCandles.AppendRange(candles);

        return Task.CompletedTask;
    }

    public Task RemoveCandles(int timeFrame)
    {
        _candles[timeFrame] = new(Array.Empty<Candle>());
        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes candles that opened before 'openedAt' date time (exclusively).
    /// </summary>
    public Task RemoveCandlesBefore(int timeFrame, DateTime openedAt)
    {
        _candles[timeFrame]?.SkipWhile(c => c.Date < openedAt);
        return Task.CompletedTask;
    }

    public Task RemoveCandlesBefore(int timeFrame, long unixTimeMilliseconds)
    {
        DateTime openedAt = DateTimeOffset.FromUnixTimeMilliseconds(unixTimeMilliseconds).DateTime;
        _candles[timeFrame]?.SkipWhile(c => c.Date < openedAt);
        return Task.CompletedTask;
    }

    public Task RemoveLastCandle(int timeFrame)
    {
        _candles[timeFrame]?.SkipLast(1);
        return Task.CompletedTask;
    }

    public Task RemoveCandles(int timeFrame, int count)
    {
        _candles[timeFrame]?.SkipLast(count);
        return Task.CompletedTask;
    }
}