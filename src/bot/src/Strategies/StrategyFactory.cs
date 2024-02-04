using bot.src.Indicators;
using bot.src.Notifiers;
using Serilog;
using bot.src.Strategies.SmmaRsi;
using bot.src.Brokers;

namespace bot.src.Strategies;

public static class StrategyFactory
{
    public static IStrategy CreateStrategy(string strategyName, IStrategyOptions strategyOptions, IIndicatorsOptions indicatorsOptions, IBroker broker, INotifier notifier, ILogger logger) => strategyName switch
    {
        "SmmaRsi" => new SmmaRsiStrategy(strategyOptions, indicatorsOptions, broker, notifier, logger),
        _ => throw new ArgumentException($"Invalid value for {nameof(strategyName)} parameter provider.")
    };
}
