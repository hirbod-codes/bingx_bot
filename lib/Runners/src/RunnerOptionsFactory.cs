using Abstractions.src.Runners;

namespace Runners.src;

public static class RunnerOptionsFactory
{
    public static IRunnerOptions CreateRunnerOptions(string optionsName) => optionsName switch
    {
        RunnerNames.SUPER_TREND_V1 => new SuperTrendV1.RunnerOptions(),
        _ => throw new Exception("Invalid runner option name provided.")
    };

    public static Type? GetInstanceType(string? name) => name switch
    {
        null => null,
        RunnerNames.SUPER_TREND_V1 => typeof(SuperTrendV1.RunnerOptions),
        _ => throw new Exception()
    };
}
