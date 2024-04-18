using Abstractions.src.Brokers;
using Abstractions.src.Models;
using Abstractions.src.Notifiers;
using Abstractions.src.Repository;
using Abstractions.src.Runners;
using Abstractions.src.Utilities;
using ILogger = Serilog.ILogger;

namespace Runners.src.CandleRepositoryRunner;

public class Runner : IRunner
{
    private const int MILLISECONDS_OFFSET = 30000;
    private readonly ICandleRepository _candleRepository;
    private readonly IBroker _broker;
    private readonly ITime _time;
    private readonly INotifier _notifier;
    private readonly ILogger _logger;
    private readonly RunnerOptions _runnerOptions;
    private System.Timers.Timer? _timer = null;
    private bool _isFirstStart = true;
    private bool _isTicking = false;

    public RunnerStatus Status { get; set; }

    public Runner(IRunnerOptions runnerOptions, ICandleRepository candleRepository, IBroker broker, ITime time, INotifier notifier, ILogger logger)
    {
        _runnerOptions = (runnerOptions as RunnerOptions)!;

        _candleRepository = candleRepository;

        _broker = broker;
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
        await SetTimer();
    }

    public Task Stop()
    {
        Status = RunnerStatus.STOPPED;
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

        await SetTimer();
    }

    private async Task SetTimer()
    {
        _timer?.Stop();
        _timer = await _time.StartTimer(300, async (o, args) => await Tick(), MILLISECONDS_OFFSET);
    }

    private async Task Tick()
    {
        DateTime tickTime = _time.GetUtcNow();

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
            DateTime limitTime = now.AddSeconds((double)(6 + (MILLISECONDS_OFFSET / 1000m)));
            Candle? candle;
            do
            {
                candle = await _broker.GetCandle();

                await Task.Delay(100);
                now = now.AddMilliseconds(100);
            } while (now <= limitTime && (candle == null || (candle != null && (now - candle.Date.AddSeconds(300)).TotalSeconds >= 3)));

            if (now > limitTime || candle == null)
            {
                _logger.Warning("Broker failed to provide latest candle on time!, skipping...");
                return;
            }

            _ = _candleRepository.AddCandle(60 * 5, candle);

            Candles candles = await _candleRepository.GetCandles(7 * 24 * 12) ?? new(Array.Empty<Candle>());

            if (candles.Count() >= (3 - 1) && tickTime.Minute % 15 == 0)
            {
                IEnumerable<Candle> candles15M = candles.Where(c => c.Date >= tickTime.AddMinutes(-15));
                candles15M = candles15M.Append(candle);
                if (candles15M.Count() == 3)
                    _ = _candleRepository.AddCandle(60 * 15, new() { Date = tickTime.AddMinutes(-15), Close = candle.Close, High = Candles.High(candles15M), Low = Candles.Low(candles15M) });
            }

            if (candles.Count() >= (12 - 1) && tickTime.Minute == 0)
            {
                IEnumerable<Candle> candles1H = candles.Where(c => c.Date >= tickTime.AddHours(-1));
                candles1H = candles1H.Append(candle);
                if (candles1H.Count() == 12)
                    _ = _candleRepository.AddCandle(60 * 60, new() { Date = tickTime.AddHours(-1), Close = candle.Close, High = Candles.High(candles1H), Low = Candles.Low(candles1H) });
            }

            if (candles.Count() >= ((4 * 12) - 1) && tickTime.Hour % 4 == 0 && tickTime.Minute == 0)
            {
                IEnumerable<Candle> candles4H = candles.Where(c => c.Date >= tickTime.AddHours(-4));
                candles4H = candles4H.Append(candle);
                if (candles4H.Count() == (4 * 12))
                    _ = _candleRepository.AddCandle(60 * 60 * 4, new() { Date = tickTime.AddHours(-4), Close = candle.Close, High = Candles.High(candles4H), Low = Candles.Low(candles4H) });
            }

            if (candles.Count() >= ((24 * 12) - 1) && tickTime.Hour == 0 && tickTime.Minute == 0)
            {
                IEnumerable<Candle> candles1D = candles.Where(c => c.Date >= tickTime.AddDays(-1));
                candles1D = candles1D.Append(candle);
                if (candles1D.Count() == (24 * 12))
                    _ = _candleRepository.AddCandle(60 * 60 * 24, new() { Date = tickTime.AddDays(-1), Close = candle.Close, High = Candles.High(candles1D), Low = Candles.Low(candles1D) });
            }

            if (candles.Count() >= ((7 * 24 * 12) - 1) && tickTime.DayOfWeek == DayOfWeek.Saturday && tickTime.Hour == 0 && tickTime.Minute == 0)
            {
                IEnumerable<Candle> candles1W = candles.Where(c => c.Date >= tickTime.AddDays(-7));
                candles1W = candles1W.Append(candle);
                if (candles1W.Count() == (7 * 24 * 12))
                    _ = _candleRepository.AddCandle(60 * 60 * 24 * 7, new() { Date = tickTime.AddDays(-7), Close = candle.Close, High = Candles.High(candles1W), Low = Candles.Low(candles1W) });
            }

            _logger.Information("Candle: {@candle}", candle);

            _logger.Information("Runner finished ticking.");
        }
        catch (BrokerException ex) { _logger.Error(ex, "A broker exception is thrown. Skipping..."); }
        catch (Exception ex)
        {
            _logger.Error(ex, "A system exception is thrown. Skipping...");

            try { Task notifierTask = _notifier.SendMessage($"A system exception is thrown. Exception message: {ex.Message}"); }
            catch (Exception ex1) { _logger.Error(ex1, "System failed to notify."); }
        }
        finally { _isTicking = false; }
    }
}
