using System.Text.Json;
using bot.src.Data.Models;

namespace bot.src.Data.InMemory;

public class CandleRepository : ICandleRepository
{
    private Candles? _candles;

    public CandleRepository() { }
    public CandleRepository(Candles candles) => _candles = candles;

    public async Task CacheCandles() => _candles = await GetCandles();

    public async Task<int> CandlesCount()
    {
        if (_candles == null)
            await CacheCandles();

        return _candles!.Count();
    }

    public async Task<Candle> GetCandle(int index)
    {
        if (_candles == null)
            await CacheCandles();

        return _candles!.ElementAt(index);
    }

    public async Task<Candles> GetCandles()
    {
        if (_candles == null)
            await CacheCandles();

        return JsonSerializer.Deserialize<Candles>(File.ReadAllText("src/bot/src/Data/fetched_data/twelvedata.json"))!;
    }

    public async Task<int> GetTimeFrame()
    {
        if (_candles == null)
            await CacheCandles();

        return _candles!.TimeFrame;
    }
}
