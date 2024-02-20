using bot.src.Indicators;
using bot.src.Notifiers;
using Serilog;
using bot.src.Brokers;
using bot.src.Data;

namespace bot.src.Strategies;

public static class StrategyFactory
{
    public static IStrategy CreateStrategy(string strategyName, IStrategyOptions strategyOptions, IIndicatorOptions indicatorsOptions, IBroker broker, INotifier notifier, IMessageRepository messageRepository, ILogger logger) => strategyName switch
    {
        StrategyNames.SMMA_RSI => new SmmaRsi.Strategy(strategyOptions, indicatorsOptions, broker, notifier, messageRepository, logger),
        StrategyNames.UT_BOT => new UtBot.Strategy(strategyOptions, indicatorsOptions, messageRepository, broker, logger),
        StrategyNames.DOUBLE_UT_BOT => new DoubleUtBot.Strategy(strategyOptions, indicatorsOptions, messageRepository, broker, logger),
        _ => throw new ArgumentException($"Invalid value for {nameof(strategyName)} parameter provider.")
    };
}
