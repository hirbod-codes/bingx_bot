using System.Text.Json;
using bot.src.Data.Models;

namespace bot.src.Data.InMemory;

public class CandleRepository : ICandleRepository
{
    private Candles _candles = new();

    public CandleRepository() { }
    public CandleRepository(Candles candles) => _candles = candles;

    public async Task CacheCandles() => _candles = await GetCandles();

    public async Task<int> CandlesCount()
    {
        if (!_candles.Any())
            await CacheCandles();

        return _candles!.Count();
    }

    public async Task<Candle> GetCandle(int index)
    {
        if (!_candles.Any())
            await CacheCandles();

        return _candles!.ElementAt(index);
    }

    public Task<Candles> GetCandles()
    {
        IEnumerable<Candle> candlesEnumerable = JsonSerializer.Deserialize<IEnumerable<Candle>>(File.ReadAllText("/home/hirbod/projects/bingx_ut_bot/src/bot/src/Data/fetched_data/twelvedata.json"), new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new Exception("No data provider");
        // candlesEnumerable = candlesEnumerable.Take(50);
        _candles.SetCandles(candlesEnumerable);
        return Task.FromResult(_candles);
    }

    public async Task<int> GetTimeFrame()
    {
        if (!_candles.Any())
            await CacheCandles();

        return _candles!.TimeFrame;
    }
}
