namespace bot.src.Strategies;

public static class StrategyOptionsFactory
{
    public static IStrategyOptions CreateStrategyOptions(string strategyName) => strategyName switch
    {
        StrategyNames.SMMA_RSI => new SmmaRsi.StrategyOptions(),
        StrategyNames.EMA_RSI => new EmaRsi.StrategyOptions(),
        StrategyNames.STOCHASTIC_EMA => new StochasticEma.StrategyOptions(),
        StrategyNames.EMA_STOCHASTIC_SUPER_TREND => new EmaStochasticSuperTrend.StrategyOptions(),
        StrategyNames.SUPER_TREND_V1 => new SuperTrendV1.StrategyOptions(),
        StrategyNames.DOUBLE_UT_BOT => new DoubleUtBot.StrategyOptions(),
        StrategyNames.UT_BOT => new UtBot.StrategyOptions(),
        StrategyNames.LUCK => new Luck.StrategyOptions(),
        StrategyNames.CANDLES_OPEN_CLOSE => new CandlesOpenClose.StrategyOptions(),
        _ => throw new InvalidStrategyNameException()
    };
}
