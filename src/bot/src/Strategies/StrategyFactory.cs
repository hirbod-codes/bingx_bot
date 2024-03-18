using bot.src.Indicators;
using bot.src.Notifiers;
using Serilog;
using bot.src.Brokers;
using bot.src.Data;
using bot.src.RiskManagement;

namespace bot.src.Strategies;

public static class StrategyFactory
{
    public static IStrategy CreateStrategy(string strategyName, IStrategyOptions strategyOptions, IIndicatorOptions indicatorsOptions, IRiskManagement riskManagement, IBroker broker, INotifier notifier, IMessageRepository messageRepository, ILogger logger) => strategyName switch
    {
        StrategyNames.SMMA_RSI => new SmmaRsi.Strategy(strategyOptions, indicatorsOptions, broker, notifier, messageRepository, logger),
        StrategyNames.EMA_STOCHASTIC_SUPER_TREND => new EmaStochasticSuperTrend.Strategy(strategyOptions, indicatorsOptions, riskManagement, broker, notifier, messageRepository, logger),
        StrategyNames.EMA_RSI => new EmaRsi.Strategy(strategyOptions, indicatorsOptions, broker, notifier, messageRepository, logger),
        StrategyNames.STOCHASTIC_EMA => new StochasticEma.Strategy(strategyOptions, indicatorsOptions, broker, notifier, messageRepository, logger),
        StrategyNames.UT_BOT => new UtBot.Strategy(strategyOptions, indicatorsOptions, messageRepository, broker, logger),
        StrategyNames.DOUBLE_UT_BOT => new DoubleUtBot.Strategy(strategyOptions, indicatorsOptions, messageRepository, broker, logger),
        StrategyNames.LUCK => new Luck.Strategy(strategyOptions, indicatorsOptions, broker, notifier, messageRepository, logger),
        StrategyNames.CANDLES_OPEN_CLOSE => new CandlesOpenClose.Strategy(strategyOptions, indicatorsOptions, broker, notifier, messageRepository, logger),
        _ => throw new ArgumentException($"Invalid value for {nameof(strategyName)} parameter provider.")
    };
}
