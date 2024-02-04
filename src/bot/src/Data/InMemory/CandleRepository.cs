using System.Text.Json;
using bot.src.Data.Models;

namespace bot.src.Data.InMemory;

public class CandleRepository : ICandleRepository
{
    private Candle _currentCandle = null!;
    private Candles _candles = new();
    private Candles _indicatorsCandles = new();

    public CandleRepository() { }
    public CandleRepository(Candles candles) => _candles = candles;

    public async Task<int> CandlesCount() => !_candles.Any() ? (await GetCandles()).Count() : _candles!.Count();

    public async Task<Candle?> GetCandle(int index) => !_candles.Any() ? (await GetCandles()).ElementAtOrDefault(index) : _candles!.ElementAtOrDefault(index);

    public Task<Candles> GetCandles()
    {
        if (_candles.Any())
            return Task.FromResult(_candles);

        IEnumerable<Candle> candles = JsonSerializer.Deserialize<IEnumerable<Candle>>(File.ReadAllText("/home/hirbod/projects/bingx_ut_bot/src/bot/src/Data/fetched_data/twelvedata.json"), new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new Exception("No data provider");
        // IEnumerable<Candle> candles = JsonSerializer.Deserialize<IEnumerable<Candle>>(File.ReadAllText("/home/hirbod/projects/bingx_ut_bot/src/bot/src/Data/fetched_data/kline_raw_data_Y-1-18__12:37:36.json"), new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new Exception("No data provider");
        // IEnumerable<Candle> candles = JsonSerializer.Deserialize<IEnumerable<Candle>>(File.ReadAllText("/home/hirbod/projects/bingx_ut_bot/src/bot/src/Data/fetched_data/kline_data_one_month_1min.json"), new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new Exception("No data provider");
        // IEnumerable<Candle> candles = JsonSerializer.Deserialize<IEnumerable<Candle>>(File.ReadAllText("/home/hirbod/projects/bingx_ut_bot/src/bot/src/Data/fetched_data/3month_kline_data.json"), new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new Exception("No data provider");
        // _indicatorsCandles.SetCandles(candles);
        _candles.SetCandles(candles);
        // _candles.SetCandles(candles.Take((int)(0.25 * candles.Count())));

        return Task.FromResult(_candles);
    }

    public async Task<Candles> GetIndicatorsCandles()
    {
        if (!_indicatorsCandles.Any())
            await GetCandles();

        return _indicatorsCandles;
    }

    public Candle GetCurrentCandle() => _currentCandle;
    public void SetCurrentCandle(Candle candle) => _currentCandle = candle;

    public async Task<int> GetTimeFrame() => _candles.Count() < 2 ? (await GetCandles()).TimeFrame : _candles!.TimeFrame;
}
