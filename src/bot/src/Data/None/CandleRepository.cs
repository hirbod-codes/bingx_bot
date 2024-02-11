using bot.src.Data.Models;

namespace bot.src.Data.None;

public class CandleRepository : ICandleRepository
{
    public Task<int> CandlesCount()
    {
        throw new NotImplementedException();
    }

    public Task<Candle> GetCandle(int index = 0)
    {
        throw new NotImplementedException();
    }

    public Task<Candles> GetCandles()
    {
        throw new NotImplementedException();
    }

    public Candle GetCurrentCandle()
    {
        throw new NotImplementedException();
    }

    public Task<Candles> GetIndicatorsCandles()
    {
        throw new NotImplementedException();
    }

    public Task<int> GetTimeFrame()
    {
        throw new NotImplementedException();
    }

    public void SetCurrentCandle(Candle candle)
    {
        throw new NotImplementedException();
    }
}
