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
    private bool _isBrokerInitiated = false;
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
                await _time.Sleep(2000);
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
        if (!_isBrokerInitiated)
        {
            _isBrokerInitiated = true;
            await _broker.InitiateCandleStore();
            _isBrokerReady = true;
            return;
        }
        else if (_isBrokerInitiated && !_isBrokerReady)
            return;

        Candles candles = await _broker.GetCandles();
        Candle candle = await _broker.GetCandle();

        _strategy.InitializeIndicators(candles);
        // await _strategy.HandleCandle(candle);

        await _bot.Tick();
    }
}
