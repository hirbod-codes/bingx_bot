using bot.src.Bots;
using bot.src.Brokers;
using bot.src.Notifiers;
using bot.src.Strategies;
using bot.src.Util;
using Serilog;

namespace bot.src.Runners;

public static class RunnerFactory
{
    public static IRunner CreateRunner(string runnerName, IRunnerOptions runnerOptions, IBot bot, IBroker broker, IStrategy strategy, ITime time, INotifier notifier, ILogger logger) => runnerName switch
    {
        RunnerNames.UT_BOT => new UtBot.Runner(runnerOptions, bot, broker, strategy, time, notifier, logger),
        RunnerNames.DOUBLE_UT_BOT => new DoubleUtBot.Runner(runnerOptions, bot, broker, strategy, time, notifier, logger),
        RunnerNames.SMMA_RSI => new SmmaRsi.Runner(runnerOptions, bot, broker, strategy, time, notifier, logger),
        RunnerNames.EMA_RSI => new EmaRsi.Runner(runnerOptions, bot, broker, strategy, time, notifier, logger),
        RunnerNames.STOCHASTIC_EMA => new EmaRsi.Runner(runnerOptions, bot, broker, strategy, time, notifier, logger),
        RunnerNames.LUCK => new Luck.Runner(runnerOptions, bot, broker, strategy, time, notifier, logger),
        _ => throw new RunnerException()
    };
}
