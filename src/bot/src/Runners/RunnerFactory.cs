using bot.src.Bots;
using bot.src.Brokers;
using bot.src.Notifiers;
using bot.src.Strategies;
using bot.src.Util;
using Serilog;
using UtBotRunner = bot.src.Runners.UtBot.Runner;
using DoubleUtBotRunner = bot.src.Runners.DoubleUtBot.Runner;
using SmmaRsiRunner = bot.src.Runners.SmmaRsi.Runner;
using EmaRsiRunner = bot.src.Runners.EmaRsi.Runner;

namespace bot.src.Runners;

public static class RunnerFactory
{
    public static IRunner CreateRunner(string runnerName, IRunnerOptions runnerOptions, IBot bot, IBroker broker, IStrategy strategy, ITime time, INotifier notifier, ILogger logger) => runnerName switch
    {
        RunnerNames.UT_BOT => new UtBotRunner(runnerOptions, bot, broker, strategy, time, notifier, logger),
        RunnerNames.DOUBLE_UT_BOT => new DoubleUtBotRunner(runnerOptions, bot, broker, strategy, time, notifier, logger),
        RunnerNames.SMMA_RSI => new SmmaRsiRunner(runnerOptions, bot, broker, strategy, time, notifier, logger),
        RunnerNames.EMA_RSI => new EmaRsiRunner(runnerOptions, bot, broker, strategy, time, notifier, logger),
        _ => throw new RunnerException()
    };
}
