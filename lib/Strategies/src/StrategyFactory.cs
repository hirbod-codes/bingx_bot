using ILogger = Serilog.ILogger;
using Abstractions.src.Strategies;
using Abstractions.src.Indicators;
using Abstractions.src.RiskManagement;
using Abstractions.src.Brokers;
using Abstractions.src.Notifiers;
using Abstractions.src.Data;

namespace Strategies.src;

public static class StrategyFactory
{
    public static IStrategy CreateStrategy(string strategyName, IStrategyOptions strategyOptions, IIndicatorOptions indicatorsOptions, IRiskManagement riskManagement, IBroker broker, INotifier notifier, IMessageRepository messageRepository, ILogger logger) => strategyName switch
    {
        StrategyNames.SUPER_TREND_V1 => new SuperTrendV1.Strategy(strategyOptions, indicatorsOptions, broker, notifier, messageRepository, logger),
        _ => throw new ArgumentException($"Invalid value for {nameof(strategyName)} parameter provider.")
    };
}
