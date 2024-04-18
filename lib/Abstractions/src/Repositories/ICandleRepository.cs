using Abstractions.src.Models;

namespace Abstractions.src.Repository;

public interface ICandleRepository
{
    public Task<Candles?> GetCandles(int timeFrame);
    public Task<Candles?> GetCandles(int timeFrame, int count);
    public Task<Candle?> GetLastCandle(int timeFrame);
    public Task<Candle?> GetCandle(int timeFrame, int index);
    public Task<Candle?> GetCandle(int timeFrame, DateTime openedAt);

    public Task AddCandle(int timeFrame, Candle candle);
    public Task AddCandles(int timeFrame, IEnumerable<Candle> candles);

    public Task RemoveCandles(int timeFrame);
    /// <summary>
    /// Removes candles that opened before 'openedAt' date time (exclusively).
    /// </summary>
    public Task RemoveCandlesBefore(int timeFrame, DateTime openedAt);
    public Task RemoveCandlesBefore(int timeFrame, long unixTimeMilliseconds);
    public Task RemoveLastCandle(int timeFrame);
    public Task RemoveCandles(int timeFrame, int count);
}