using bot.src.MessageStores;
using bot.src.Util;
using Serilog;
using GeneralStrategyClass = bot.src.Strategies.GeneralStrategy.GeneralStrategy;

namespace bot.src.Strategies;

public static class StrategyFactory
{
    public static IStrategy CreateStrategy(string strategyName, IMessageStore messageStore, string provider, ILogger logger, ITime time) => strategyName switch
    {
        "General" => new GeneralStrategyClass(provider, messageStore, logger, time),
        _ => throw new Exception()
    };
}
