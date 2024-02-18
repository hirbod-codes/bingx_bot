using bot.src.Bots;
using bot.src.Data.Models;
using bot.src.Strategies;
using Serilog;
using Skender.Stock.Indicators;
using StrategyTester.src.Utils;

namespace StrategyTester.src.Testers.General;

public class GeneralTester : ITester
{
    private readonly TesterOptions _testerOptions;
    private readonly ITime _time;
    private readonly IStrategy _strategy;
    private readonly Brokers.IBroker _broker;
    private readonly IBot _bot;
    private readonly ILogger _logger;

    public GeneralTester(ITesterOptions testerOptions, ITime time, IStrategy strategy, Brokers.IBroker broker, IBot bot, ILogger logger)
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
        _logger.Information("Testing...");

        await _broker.InitiateCandleStore();

        Candles candles = await _broker.GetCandles();

        _strategy.PrepareIndicators(candles);

        while (!_broker.IsFinished())
        {
            if (await _broker.GetLastCandleIndex() < 203)
            {
                _broker.NextCandle();
                continue;
            }

            Candle candle = await _broker.GetCandle();

            _logger.Information("candle: {@candle}", candle);

            _time.SetUtcNow(candle.Date.AddSeconds(_testerOptions.TimeFrame));

            try { await _strategy.HandleCandle(candle, _testerOptions.TimeFrame); }
            catch (NotEnoughCandlesException)
            {
                _broker.NextCandle();
                continue;
            }

            await _broker.CandleClosed();

            await _bot.Tick();

            _broker.NextCandle();
        }

        _logger.Information("Finished testing...");
    }
}
