using bot.src.Strategies.SmmaRsi;

namespace bot.src.Strategies;

public static class StrategyOptionsFactory
{
    public static IStrategyOptions CreateStrategyOptions(string strategyName) => strategyName switch
    {
        StrategyNames.SMMA_RSI => new StrategyOptions(),
        _ => throw new InvalidStrategyNameException()
    };
}
