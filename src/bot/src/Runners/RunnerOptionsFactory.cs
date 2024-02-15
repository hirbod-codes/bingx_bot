using UtBotRunnerOptions = bot.src.Runners.UtBot.RunnerOptions;

namespace bot.src.Runners;

public static class RunnerOptionsFactory
{
    public static IRunnerOptions CreateRunnerOptions(string optionsName) => optionsName switch
    {
        RunnerOptionsNames.UT_BOT => new UtBotRunnerOptions(),
        _ => throw new Exception("Invalid runner option name provided.")
    };
}
