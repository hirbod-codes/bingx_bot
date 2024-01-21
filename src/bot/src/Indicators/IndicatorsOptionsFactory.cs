using bot.src.Indicators.SmmaRsi;

namespace bot.src.Indicators;

public static class IndicatorsOptionsFactory
{
    public static IIndicatorsOptions CreateIndicatorOptions(string IndicatorsOptionsName) => IndicatorsOptionsName switch
    {
        IndicatorsOptionsNames.SMMA_RSI => new IndicatorsOptions(),
        _ => throw new Exception()
    };
}
