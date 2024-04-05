namespace bot.src.Indicators;

public static class IndicatorOptionsFactory
{
    public static IIndicatorOptions CreateIndicatorOptions(string indicatorOptionsName) => indicatorOptionsName switch
    {
        IndicatorsOptionsNames.SUPER_TREND_V1 => new SuperTrendV1.IndicatorOptions(),
        _ => throw new Exception("Invalid Indicator options name provided.")
    };

    public static Type? GetInstanceType(string? name) => name switch
    {
        null => null,
        IndicatorsOptionsNames.SUPER_TREND_V1 => typeof(SuperTrendV1.IndicatorOptions),
        _ => throw new Exception()
    };
}
