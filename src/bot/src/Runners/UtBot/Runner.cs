using bot.src.Bots;
using bot.src.Brokers;
using bot.src.Data.Models;
using bot.src.Notifiers;
using bot.src.Strategies;
using bot.src.Util;
using Serilog;

namespace bot.src.Runners.UtBot;

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
        _logger = logger;
    }

    public async Task Run()
    {
        try
        {
            _logger.Information("Runner started at: {dateTime}", DateTime.UtcNow.ToString());
            await _notifier.SendMessage($"Runner started at: {DateTime.UtcNow}");

            await _time.StartTimer(_runnerOptions.TimeFrame, async (o, args) =>
            {
                await _time.Sleep(2500);
                await Tick();
            });

            while (true)
            {
                string? r = System.Console.ReadLine();

                if (r is not null && r.ToLower() == "exit")
                    return;
            }
        }
        finally
        {
            _logger.Information("Runner terminated at: {dateTime}", DateTime.UtcNow.ToString());
            await _notifier.SendMessage($"Runner terminated at: {DateTime.UtcNow}");
        }
    }

    private async Task Tick()
    {
        if (!_hasBrokerInitiated)
        {
            _hasBrokerInitiated = true;
            await _broker.InitiateCandleStore(30000);
            _isBrokerReady = true;
            return;
        }
        else if (_hasBrokerInitiated && !_isBrokerReady)
            return;

        Candle candle = await _broker.GetCandle();

        DateTime now = DateTime.UtcNow;
        if ((candle.Date - now.AddSeconds(now.Second * -1).AddSeconds(_runnerOptions.TimeFrame * -1)).TotalSeconds >= 1)
            return;

        await _strategy.HandleCandle(candle, _runnerOptions.TimeFrame);

        await _bot.Tick();
    }
}
