using bot.src.Data;
using bot.src.Indicators;
using bot.src.Notifiers;
using bot.src.RiskManagement;
using Serilog;
using bot.src.Strategies.SmmaRsi;

namespace bot.src.Strategies;

public static class StrategyFactory
{
    public static IStrategy CreateStrategy(string strategyName, ICandleRepository candleRepository, IStrategyOptions strategyOptions, IIndicatorsOptions indicatorsOptions, INotifier notifier, ILogger logger) => strategyName switch
    {
        "SmmaRsi" => new SmmaRsiStrategy(candleRepository, strategyOptions, indicatorsOptions, notifier, logger),
        _ => throw new ArgumentException($"Invalid value for {nameof(strategyName)} parameter provider.")
    };
}
