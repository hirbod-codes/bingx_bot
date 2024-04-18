using Abstractions.src.Bots;
using Abstractions.src.Brokers;
using Abstractions.src.Indicators;
using Abstractions.src.MessageStore;
using Abstractions.src.Notifiers;
using Abstractions.src.RiskManagements;
using Abstractions.src.Runners;
using Abstractions.src.Strategies;

namespace Abstractions.src.Models;

public class Options
{
    public int? TimeFrame { get; set; }
    public INotifierOptions? NotifierOptions { get; set; }
    public IMessageStoreOptions? MessageStoreOptions { get; set; }
    public IBrokerOptions? BrokerOptions { get; set; }
    public IRiskManagementOptions? RiskManagementOptions { get; set; }
    public IIndicatorOptions? IndicatorOptions { get; set; }
    public IStrategyOptions? StrategyOptions { get; set; }
    public IBotOptions? BotOptions { get; set; }
    public IRunnerOptions? RunnerOptions { get; set; }
}
