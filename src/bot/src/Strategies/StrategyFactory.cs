using bot.src.Indicators;
using bot.src.Notifiers;
using Serilog;
using bot.src.Strategies.SmmaRsi;
using bot.src.Brokers;
using bot.src.Data;
using bot.src.Strategies.UtBot;

namespace bot.src.Strategies;

public static class StrategyFactory
{
    public static IStrategy CreateStrategy(string strategyName, IStrategyOptions strategyOptions, IIndicatorOptions indicatorsOptions, IBroker broker, INotifier notifier, IMessageRepository messageRepository, ILogger logger) => strategyName switch
    {
        StrategyNames.SMMA_RSI => new SmmaRsiStrategy(strategyOptions, indicatorsOptions, broker, notifier, messageRepository, logger),
        StrategyNames.UT_BOT => new UtBotStrategy(strategyOptions, indicatorsOptions, messageRepository, broker, logger),
        _ => throw new ArgumentException($"Invalid value for {nameof(strategyName)} parameter provider.")
    };
}
