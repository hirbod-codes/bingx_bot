namespace bot.src.Strategies;

public static class StrategyOptionsFactory
{
    public static IStrategyOptions CreateStrategyOptions(string strategyName) => strategyName switch
    {
        StrategyNames.SUPER_TREND_V1 => new SuperTrendV1.StrategyOptions(),
        _ => throw new InvalidStrategyNameException()
    };

    public static Type? GetInstanceType(string? name) => name switch
    {
        null => null,
        StrategyNames.SUPER_TREND_V1 => typeof(SuperTrendV1.StrategyOptions),
        _ => throw new Exception()
    };
}
