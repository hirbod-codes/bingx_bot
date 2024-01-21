using bot.src.Data;
using bot.src.Indicators;
using bot.src.Notifiers;
using bot.src.RiskManagement;
using Serilog;
using Skender.Stock.Indicators;
using bot.src.Strategies.General;

namespace bot.src.Strategies;

public static class StrategyFactory
{
    public static IStrategy CreateStrategy(string strategyName, ICandleRepository candleRepository, IIndicatorsOptions indicatorsOptions, INotifier notifier, IRiskManagement riskManagement, ILogger logger) => strategyName switch
    {
        "SmmaRsi" => new SmmaRsiStrategy(candleRepository, indicatorsOptions, notifier, riskManagement, logger),
        _ => throw new ArgumentException($"Invalid value for {nameof(strategyName)} parameter provider.")
    };
}
