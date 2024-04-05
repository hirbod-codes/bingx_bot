using bot.src.Bots;
using bot.src.Brokers;
using bot.src.Notifiers;
using bot.src.Strategies;
using bot.src.Util;
using ILogger = Serilog.ILogger;

namespace bot.src.Runners;

public static class RunnerFactory
{
    public static IRunner CreateRunner(string runnerName, IRunnerOptions runnerOptions, IBot bot, IBroker broker, IStrategy strategy, ITime time, INotifier notifier, ILogger logger) => runnerName switch
    {
        RunnerNames.SUPER_TREND_V1 => new SuperTrendV1.Runner(runnerOptions, bot, broker, strategy, time, notifier, logger),
        _ => throw new RunnerException()
    };
}
