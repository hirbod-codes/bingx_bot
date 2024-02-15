using SmmaRsiIndicatorOptions = bot.src.Indicators.SmmaRsi.IndicatorOptions;
using UtBotIndicatorOptions = bot.src.Indicators.UtBot.IndicatorOptions;


namespace bot.src.Indicators;

public static class IndicatorOptionsFactory
{
    public static IIndicatorOptions CreateIndicatorOptions(string indicatorOptionsName) => indicatorOptionsName switch
    {
        IndicatorsOptionsNames.SMMA_RSI => new SmmaRsiIndicatorOptions(),
        IndicatorsOptionsNames.UT_BOT => new UtBotIndicatorOptions(),
        _ => throw new Exception("Invalid Indicator options name provided.")
    };
}
