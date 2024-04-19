using Abstractions.src.Bots;
using Abstractions.src.Brokers;
using Abstractions.src.Notifiers;
using Abstractions.src.Runners;
using Abstractions.src.Strategies;
using Abstractions.src.Utilities;
using ILogger = Serilog.ILogger;

namespace Runners.src;

public static class RunnerFactory
{
    public static IRunner CreateRunner(string runnerName, IRunnerOptions runnerOptions, IBot bot, IBroker broker, IStrategy strategy, ITime time, INotifier notifier, ILogger logger) => runnerName switch
    {
        RunnerNames.SUPER_TREND_V1 => new SuperTrendV1.Runner(runnerOptions, bot, broker, strategy, time, notifier, logger),
        _ => throw new RunnerException()
    };
}
