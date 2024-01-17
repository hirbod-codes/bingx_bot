using System.Text.Json;
using bot.src.Data.Models;

namespace bot.src.Data.InMemory;

public class CandleRepository : ICandleRepository
{
    private Candles _candles = new();

    public CandleRepository() { }
    public CandleRepository(Candles candles) => _candles = candles;

    public async Task<int> CandlesCount() => !_candles.Any() ? (await GetCandles()).Count() : _candles!.Count();

    public async Task<Candle> GetCandle(int index) => !_candles.Any() ? (await GetCandles()).ElementAt(index) : _candles!.ElementAt(index);

    public Task<Candles> GetCandles()
    {
        if (_candles.Any())
            return Task.FromResult(_candles);

        IEnumerable<Candle> candlesEnumerable = JsonSerializer.Deserialize<IEnumerable<Candle>>(File.ReadAllText("/home/hirbod/projects/bingx_ut_bot/src/bot/src/Data/fetched_data/twelvedata.json"), new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new Exception("No data provider");
        _candles.SetCandles(candlesEnumerable);

        return Task.FromResult(_candles);
    }

    public async Task<int> GetTimeFrame() => _candles.Count() < 2 ? (await GetCandles()).TimeFrame : _candles!.TimeFrame;
}
