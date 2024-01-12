using System.Runtime.InteropServices;
using System.Text.Json;
using System.Timers;
using bot.src.Util;
using manual_tests.bingx;
using manual_tests.bingx.Providers;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace manual_tests;

public class UnitTest1
{
    private readonly string _dir = "/home/hirbod/projects/bingx_ut_bot/Tests/manual_tests";

    [Fact]
    public async Task Test2()
    {
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddJsonFile($"{_dir}/coinex/confs.json").Build();

        ILogger logger = new LoggerConfiguration().CreateLogger();

        coinex.Account account = new(configurationRoot["BaseUrl"]!, configurationRoot["ApiKey"]!, configurationRoot["ApiSecret"]!, configurationRoot["Symbol"]!, logger);

        float balance = await account.GetBalance();
    }

    [Fact]
    public async Task Test1()
    {
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddJsonFile($"{_dir}/bingx/appsettings_old.json").Build();

        ILogger logger = new LoggerConfiguration().CreateLogger();

        Market market = new(configurationRoot["UT:BingxApi:BaseUrl"]!, configurationRoot["UT:BingxApi:ApiKey"]!, configurationRoot["UT:BingxApi:ApiSecret"]!, configurationRoot["UT:BingxApi:Symbol"]!, new BingxUtilities(logger));
        await market.GetKLineData(_dir);

        BingxResponse<IEnumerable<BingxCandle>> bingxResponse = JsonSerializer.Deserialize<BingxResponse<IEnumerable<BingxCandle>>>(File.ReadAllText($"{_dir}/bingx/kline_data.json"), new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new NullReferenceException();

        List<Candle> candles = bingxResponse.Data!.ToList().ConvertAll(o =>
            new Candle()
            {
                Open = float.Parse(o.Open),
                Close = float.Parse(o.Close),
                High = float.Parse(o.High),
                Low = float.Parse(o.Low),
                Volume = float.Parse(o.Volume),
                DateTime = DateTimeOffset.FromUnixTimeMilliseconds(o.Time).DateTime,
            }
        );

        return;
    }

    public static List<object[]> TestRM1Data = new(){
        new object[]
        {
            44000,
            135d * 4,
            0.0005d,
            0.7d,
            40,
            0.25
        }
    };

    [Theory]
    [MemberData(nameof(TestRM1Data))]
    public void TestRM1(double lastPrice, double delta, double commission, double maxBingxSl, double loss, double ratio)
    {
        double leverage = maxBingxSl / ((delta / lastPrice) - commission);

        if (leverage - Math.Floor(leverage) != 0)
        {
            leverage = Math.Floor(leverage);
            maxBingxSl = leverage * ((delta / lastPrice) - commission);
        }

        double sl = maxBingxSl + (commission * leverage);
        double bingxTp = ((delta * ratio) + (commission * lastPrice)) * leverage / lastPrice;

        double margin;
        if (sl <= 0.7)
            margin = loss / sl;
        else
            margin = loss / 0.7;

        File.WriteAllTextAsync($"{_dir}/aaaa", $@"leverage: {leverage}
sl: {sl}
margin: {margin}
maxBingxSl: {maxBingxSl}
bingxTp: {bingxTp}
short:
    tpPrice: {lastPrice - (delta * ratio)}
    slPrice: {lastPrice + delta}
");
    }

    public static List<object[]> TestRM2Data = new(){
        new object[]
        {
            44000d,
            44000d - (135d * 4),
            44000d + 135d,
            0.0005d,
            0.7d,
            40
        }
    };

    [Theory]
    [MemberData(nameof(TestRM2Data))]
    public async Task TestRM2(double entryPrice, double slPrice, double tpPrice, double commission, double maxBingxSl, double loss)
    {
        if (!(tpPrice > entryPrice && entryPrice > slPrice) && !(slPrice > entryPrice && entryPrice > tpPrice))
            throw new Exception();

        double slDelta = Math.Abs(entryPrice - slPrice);
        double tpDelta = Math.Abs(entryPrice - tpPrice);

        double leverage = maxBingxSl * entryPrice / (slDelta + (commission * entryPrice));

        double sl;
        if (leverage - Math.Floor(leverage) != 0)
        {
            leverage = Math.Ceiling(leverage) + 2;
            do
            {
                leverage--;
                sl = leverage * (slDelta + (commission * entryPrice)) / entryPrice;
            } while (sl > maxBingxSl);
        }
        else
            sl = maxBingxSl;

        double bingxTp = ((tpDelta * leverage) - (commission * leverage * entryPrice)) / entryPrice;

        double bingxSl = slDelta * leverage / entryPrice;

        double margin = loss / sl;

        await File.WriteAllTextAsync($"{_dir}/logs/{DateTime.UtcNow.ToString("H:mm:ss")}", $@"leverage: {leverage}
{nameof(margin)}: {margin}
{nameof(sl)}: {sl}
{nameof(bingxSl)}: {bingxSl}
{nameof(bingxTp)}: {bingxTp}
{nameof(tpDelta)}: {tpDelta}
{nameof(slDelta)}: {slDelta}
short:
    tp: {entryPrice - tpDelta}
    sl: {entryPrice + slDelta}
long:
    tp: {entryPrice + tpDelta}
    sl: {entryPrice - slDelta}
");
    }

    [Fact]
    public async Task TestTimer()
    {
        File.WriteAllText($"{_dir}/logs/timer.log", "");

        var t = new Time().StartTimer(61, new ElapsedEventHandler((o, args) =>
        {
            DateTime dt = DateTime.UtcNow;
            File.AppendAllText($"{_dir}/logs/timer.log", $"\n{dt.TimeOfDay}--------------{dt.Ticks}");
        }));

        await Task.Delay(10 * 60 * 1000);
    }

    private static double GetInterval()
    {
        DateTime now = DateTime.UtcNow;
        return (60 - now.Second) * 1000 - now.Millisecond;
    }
}

public class BingxResponse<T>
{
    public int Code { get; set; }
    public string Msg { get; set; } = null!;
    public T? Data { get; set; }
}

public class BingxCandle
{
    public string Open { get; set; } = null!;
    public string Close { get; set; } = null!;
    public string High { get; set; } = null!;
    public string Low { get; set; } = null!;
    public string Volume { get; set; } = null!;
    public long Time { get; set; }
}

public class Candle
{
    public float Open { get; set; }
    public float Close { get; set; }
    public float High { get; set; }
    public float Low { get; set; }
    public float Volume { get; set; }
    public DateTime DateTime { get; set; }
}
