using Abstractions.src.Bots;
using Abstractions.src.Brokers;
using Abstractions.src.Data.Models;
using Abstractions.src.Strategies;
using Abstractions.src.Strategies.Exceptions;
using Abstractions.src.Utilities;
using ILogger = Serilog.ILogger;

namespace StrategyTester.src.Testers.General;

public class GeneralTester : ITester
{
    private readonly TesterOptions _testerOptions;
    private readonly ITimeSimulator _time;
    private readonly IStrategy _strategy;
    private readonly IBrokerSimulator _broker;
    private readonly IBot _bot;
    private readonly ILogger _logger;

    public GeneralTester(ITesterOptions testerOptions, ITimeSimulator time, IStrategy strategy, IBrokerSimulator broker, IBot bot, ILogger logger)
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

        await _broker.InitiateCandleStore(_testerOptions.CandlesCount, _testerOptions.TimeFrame);

        await _strategy.PrepareIndicators();

        while (!_broker.IsFinished())
        {
            if (await _broker.GetLastCandleIndex() < 203)
            {
                _broker.NextCandle();
                continue;
            }

            Candle candle = await _broker.GetCandle() ?? throw new TesterException();

            _logger.Information("candle: {@candle}", candle);

            _time.SetUtcNow(candle.Date.AddSeconds(_testerOptions.TimeFrame));

            await _broker.CandleClosed();

            try { await _strategy.HandleCandle(candle, _testerOptions.TimeFrame); }
            catch (NotEnoughCandlesException)
            {
                _broker.NextCandle();
                continue;
            }

            await _bot.Tick();

            _broker.NextCandle();
        }

        _logger.Information("Finished testing...");
    }
}
