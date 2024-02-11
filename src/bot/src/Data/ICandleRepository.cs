using bot.src.Data.Models;

namespace bot.src.Data;

public interface ICandleRepository
{
    public Task<int> CandlesCount();
    public Task<Candles> GetCandles();
    public Task<Candles> GetIndicatorsCandles();
    public Candle GetCurrentCandle();
    public void SetCurrentCandle(Candle candle);
    public Task<Candle> GetCandle(int index = 0);
    public Task<int> GetTimeFrame();
}
