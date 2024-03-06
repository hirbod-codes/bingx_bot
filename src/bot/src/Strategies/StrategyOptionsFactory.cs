using EmaRsiStrategyOptions = bot.src.Strategies.EmaRsi.StrategyOptions;
using SmmaRsiStrategyOptions = bot.src.Strategies.SmmaRsi.StrategyOptions;
using DoubleUtBotStrategyOptions = bot.src.Strategies.DoubleUtBot.StrategyOptions;
using UtBotStrategyOptions = bot.src.Strategies.UtBot.StrategyOptions;

namespace bot.src.Strategies;

public static class StrategyOptionsFactory
{
    public static IStrategyOptions CreateStrategyOptions(string strategyName) => strategyName switch
    {
        StrategyNames.SMMA_RSI => new SmmaRsiStrategyOptions(),
        StrategyNames.EMA_RSI => new EmaRsiStrategyOptions(),
        StrategyNames.DOUBLE_UT_BOT => new DoubleUtBotStrategyOptions(),
        StrategyNames.UT_BOT => new UtBotStrategyOptions(),
        _ => throw new InvalidStrategyNameException()
    };
}
