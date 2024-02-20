using SmmaRsiRunnerOptions = bot.src.Runners.SmmaRsi.RunnerOptions;
using UtBotRunnerOptions = bot.src.Runners.UtBot.RunnerOptions;
using DoubleUtBotRunnerOptions = bot.src.Runners.DoubleUtBot.RunnerOptions;

namespace bot.src.Runners;

public static class RunnerOptionsFactory
{
    public static IRunnerOptions CreateRunnerOptions(string optionsName) => optionsName switch
    {
        RunnerOptionsNames.SMMA_RSI => new SmmaRsiRunnerOptions(),
        RunnerOptionsNames.UT_BOT => new UtBotRunnerOptions(),
        RunnerOptionsNames.DOUBLE_UT_BOT => new DoubleUtBotRunnerOptions(),
        _ => throw new Exception("Invalid runner option name provided.")
    };
}
