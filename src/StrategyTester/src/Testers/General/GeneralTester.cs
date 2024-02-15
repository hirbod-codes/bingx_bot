using bot.src.Bots;
using bot.src.Brokers;
using bot.src.Data.Models;
using bot.src.Strategies;
using Serilog;
using StrategyTester.src.Utils;

namespace StrategyTester.src.Testers.General;

public class GeneralTester : ITester
{
    private readonly TesterOptions _testerOptions;
    private readonly ITime _time;
    private readonly IStrategy _strategy;
    private readonly IBroker _broker;
    private readonly IBot _bot;
    private readonly ILogger _logger;

    public GeneralTester(ITesterOptions testerOptions, ITime time, IStrategy strategy, IBroker broker, IBot bot, ILogger logger)
    {
        _testerOptions = (testerOptions as TesterOptions)!;
        _time = time;
        _strategy = strategy;
        _broker = broker;
        _bot = bot;
        _logger = logger.ForContext<GeneralTester>();
    }

    public async Task Test()
    {
        await _broker.InitiateCandleStore(30000);

        Candles candles = await _broker.GetCandles();

        DateTime previousCandleDate = candles.First().Date;
        while (true)
        {
            await Task.Delay(1);

            Candle candle = await _broker.GetCandle();

            if (candle.Date == previousCandleDate)
                continue;
            else
                previousCandleDate = candle.Date;

            _logger.Information("candle: {@candle}", candle);

            await _strategy.HandleCandle(candle, _testerOptions.TimeFrame);

            await _broker.CandleClosed();
            await _bot.Tick();

            _time.SetUtcNow(previousCandleDate.AddSeconds(1));
        }
    }
}
