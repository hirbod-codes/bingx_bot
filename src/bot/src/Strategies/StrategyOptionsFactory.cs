namespace bot.src.Strategies;

public static class StrategyOptionsFactory
{
    public static IStrategyOptions CreateStrategyOptions(string strategyName) => strategyName switch
    {
        StrategyNames.SMMA_RSI => new SmmaRsi.StrategyOptions(),
        StrategyNames.EMA_RSI => new EmaRsi.StrategyOptions(),
        StrategyNames.STOCHASTIC_EMA => new StochasticEma.StrategyOptions(),
        StrategyNames.DOUBLE_UT_BOT => new DoubleUtBot.StrategyOptions(),
        StrategyNames.UT_BOT => new UtBot.StrategyOptions(),
        _ => throw new InvalidStrategyNameException()
    };
}
