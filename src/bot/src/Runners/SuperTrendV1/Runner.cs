using bot.src.Bots;
using bot.src.Brokers;
using bot.src.Data.Models;
using bot.src.Notifiers;
using bot.src.Strategies;
using bot.src.Util;
using ILogger = Serilog.ILogger;

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
    private System.Timers.Timer? _timer = null;
    private bool _isFirstStart = true;
    private int _millisecondsOffset = 30000;
    private bool _isTicking = false;

    public RunnerStatus Status { get; set; }

    public Runner(IRunnerOptions runnerOptions, IBot bot, IBroker broker, IStrategy strategy, ITime time, INotifier notifier, ILogger logger)
    {
        _runnerOptions = (runnerOptions as RunnerOptions)!;
        _bot = bot;
        _broker = broker;
        _strategy = strategy;
        _time = time;
        _notifier = notifier;
        _logger = logger.ForContext<Runner>();

        Status = RunnerStatus.STOPPED;
    }

    public async Task Continue()
    {
        if (Status == RunnerStatus.RUNNING)
            return;

        if (_isFirstStart)
        {
            await Run();
            // This statement must come after Run method is call.
            Status = RunnerStatus.RUNNING;
            _isFirstStart = false;
            return;
        }

        Status = RunnerStatus.RUNNING;
        if (_runnerOptions.TimeFrame <= 60)
            _broker.StartListening();
        await SetTimer();
    }

    public Task Stop()
    {
        Status = RunnerStatus.STOPPED;
        if (_runnerOptions.TimeFrame <= 60)
            _broker.StopListening();
        _timer?.Stop();

        return Task.CompletedTask;
    }

    public Task Suspend()
    {
        Status = RunnerStatus.SUSPENDED;

        return Task.CompletedTask;
    }

    public async Task Run()
    {
        if (Status == RunnerStatus.RUNNING)
        {
            _logger.Debug("Runner is already running, Skipping...");
            return;
        }

        _logger.Information("Runner started at: {dateTime}", _time.GetUtcNow().ToString());
        _ = _notifier.SendMessage($"Runner started at: {_time.GetUtcNow()}");

        await _broker.InitiateCandleStore(_runnerOptions.HistoricalCandlesCount);
        await SetTimer();
    }

    private async Task SetTimer()
    {
        _timer?.Stop();
        if (_runnerOptions.TimeFrame <= 60)
            _timer = await _time.StartTimer(_runnerOptions.TimeFrame, async (o, args) => await Tick());
        else
            _timer = await _time.StartTimer(_runnerOptions.TimeFrame, async (o, args) => await Tick(), _millisecondsOffset);
    }

    private async Task Tick()
    {
        if (Status != RunnerStatus.RUNNING)
        {
            _logger.Information("The bot is not running, status: {status}, Skipping...", Status.ToString());
            return;
        }

        try
        {
            if (_isTicking)
                return;
            else
                _isTicking = true;

            _logger.Information("Runner's ticking...");

            DateTime now = _time.GetUtcNow();
            DateTime limitTime = now.AddSeconds((double)(6 + (_millisecondsOffset / 1000m)));
            Candle? candle = null;
            Task<Candle?>? candleTask = null;
            do
            {
                candleTask ??= _broker.GetCandle();

                await Task.Delay(100);
                now = now.AddMilliseconds(100);

                if (candleTask.IsCompleted || candleTask.IsCanceled || candleTask.IsFaulted)
                    if (candleTask.IsCompletedSuccessfully && candleTask.Result != null)
                    {
                        candle = candleTask.Result;
                        break;
                    }
                    else
                        candleTask = null;
            } while (now <= limitTime);

            _broker.StopListening();

            if (now > limitTime || candle == null || (_time.GetUtcNow() - candle.Date.AddSeconds(_runnerOptions.TimeFrame)).TotalSeconds >= 6)
            {
                _logger.Warning("Broker failed to provide latest candle on time!, skipping...");
                return;
            }

            _logger.Information("Candle: {@candle}", candle);

            if (Status == RunnerStatus.SUSPENDED)
            {
                _logger.Information("Runner is Suspended, Skipping...");
                return;
            }

            await _strategy.PrepareIndicators();

            await _strategy.HandleCandle(candle, _runnerOptions.TimeFrame);

            await _bot.Tick();

            _logger.Information("Runner finished ticking.");
        }
        catch (BrokerException ex) { _logger.Error(ex, "A broker exception is thrown. Skipping..."); }
        catch (StrategyException ex) { _logger.Error(ex, "A strategy exception is thrown. Skipping..."); }
        catch (BotException ex) { _logger.Error(ex, "A bot exception is thrown. Skipping..."); }
        catch (Exception ex)
        {
            _logger.Error(ex, "A system exception is thrown. Skipping...");

            try { Task notifierTask = _notifier.SendMessage($"A system exception is thrown. Exception message: {ex.Message}"); }
            catch (Exception ex1) { _logger.Error(ex1, "System failed to notify."); }
        }
        finally { _isTicking = false; }
    }
}
