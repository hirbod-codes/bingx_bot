using SmmaRsiStrategyOptions = bot.src.Strategies.SmmaRsi.StrategyOptions;
using UtBotStrategyOptions = bot.src.Strategies.UtBot.StrategyOptions;

namespace bot.src.Strategies;

public static class StrategyOptionsFactory
{
    public static IStrategyOptions CreateStrategyOptions(string strategyName) => strategyName switch
    {
        StrategyNames.SMMA_RSI => new SmmaRsiStrategyOptions(),
        StrategyNames.UT_BOT => new UtBotStrategyOptions(),
        _ => throw new InvalidStrategyNameException()
    };
}
