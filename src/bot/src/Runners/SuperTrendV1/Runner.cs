using bot.src.Bots;
using bot.src.Brokers;
using bot.src.Data.Models;
using bot.src.Notifiers;
using bot.src.Strategies;
using bot.src.Util;
using Serilog;

namespace bot.src.Runners.SuperTrendV1;

public class Runner : IRunner
{
    private readonly IBot _bot;
    private readonly IBroker _broker;
    private readonly IStrategy _strategy;
    private readonly ITime _time;
    private readonly INotifier _notifier;
    private readonly ILogger _logger;
    private readonly RunnerOptions _runnerOptions;
    private bool _hasBrokerInitiated = false;
    private bool _isBrokerReady = false;

    public Runner(IRunnerOptions runnerOptions, IBot bot, IBroker broker, IStrategy strategy, ITime time, INotifier notifier, ILogger logger)
    {
        _runnerOptions = (runnerOptions as RunnerOptions)!;
        _bot = bot;
        _broker = broker;
        _strategy = strategy;
        _time = time;
        _notifier = notifier;
        _logger = logger.ForContext<Runner>();
    }

    public async Task Run()
    {
        _logger.Information("Runner started at: {dateTime}", _time.GetUtcNow().ToString());
        await _notifier.SendMessage($"Runner started at: {_time.GetUtcNow()}");

        await Tick();

        await _time.StartTimer(_runnerOptions.TimeFrame, async (o, args) =>
        {
            // Waiting for broker to get the last candle.
            // await _time.Sleep(500);
            await Tick();
        });

        while (true)
        {
            string? r = System.Console.ReadLine();

            if (r is not null && r.ToLower() == "exit")
                return;
        }
    }

    private async Task Tick()
    {
        try
        {
            _logger.Information("Runner's ticking...");

            if (!_hasBrokerInitiated)
            {
                _logger.Information("The broker has not initiated, skipping...");
                _hasBrokerInitiated = true;
                await _broker.InitiateCandleStore(_runnerOptions.HistoricalCandlesCount);
                _isBrokerReady = true;
                return;
            }
            else if (_hasBrokerInitiated && !_isBrokerReady)
            {
                _logger.Information("The broker has not finished initiation, skipping...");
                return;
            }

            Candle candle;
            do
            {
                candle = await _broker.GetCandle();
            } while ((_time.GetUtcNow() - candle.Date.AddSeconds(_runnerOptions.TimeFrame)).TotalSeconds > 5);

            _logger.Information("Candle: {@candle}", candle);

            DateTime now = _time.GetUtcNow();
            if ((candle.Date - now.AddSeconds(now.Second * -1).AddSeconds(_runnerOptions.TimeFrame * -1)).TotalSeconds >= 1)
            {
                _logger.Information("now: {now}", now);
                _logger.Information("Candles missing, skipping...");
                return;
            }

            await _strategy.PrepareIndicators();

            await _strategy.HandleCandle(candle, _runnerOptions.TimeFrame);

            await _bot.Tick();

            _logger.Information("Runner finished ticking.");
        }
        catch (BrokerException ex)
        {
            _logger.Information("Broker Failed. Skipping...");
            _logger.Information("The exception message: {message}", ex.Message);
        }
    }
}
