using bot.src.Data.Models;

namespace bot.src.Data;

public interface ICandleRepository
{
    public Task<int> CandlesCount();
    public Task<Candles> GetCandles();
    public Task<Candle> GetCandle(int index);
    public Task<int> GetTimeFrame();
}
