namespace bot.src.Indicators;

public static class IndicatorOptionsFactory
{
    public static IIndicatorOptions CreateIndicatorOptions(string indicatorOptionsName) => indicatorOptionsName switch
    {
        IndicatorsOptionsNames.SMMA_RSI => new SmmaRsi.IndicatorOptions(),
        IndicatorsOptionsNames.EMA_RSI => new EmaRsi.IndicatorOptions(),
        IndicatorsOptionsNames.DOUBLE_UT_BOT => new DoubleUtBot.IndicatorOptions(),
        IndicatorsOptionsNames.UT_BOT => new UtBot.IndicatorOptions(),
        IndicatorsOptionsNames.STOCHASTIC_EMA => new StochasticEma.IndicatorOptions(),
        IndicatorsOptionsNames.EMA_STOCHASTIC_SUPER_TREND => new EmaStochasticSuperTrend.IndicatorOptions(),
        IndicatorsOptionsNames.LUCK => new Luck.IndicatorOptions(),
        IndicatorsOptionsNames.SUPER_TREND_V1 => new SuperTrendV1.IndicatorOptions(),
        IndicatorsOptionsNames.CANDLES_OPEN_CLOSE => new CandlesOpenClose.IndicatorOptions(),
        _ => throw new Exception("Invalid Indicator options name provided.")
    };
}
