using Abstractions.src.RiskManagement;
using Abstractions.src.Brokers;
using Abstractions.src.Bots;
using Abstractions.src.Runners;
using Abstractions.src.Indicators;
using Abstractions.src.Strategies;
using Abstractions.src.MessageStores;

namespace Abstractions.src.Models;

public class Options
{
    public IMessageStoreOptions? MessageStoreOptions { get; set; }
    public IBrokerOptions? BrokerOptions { get; set; }
    public IRiskManagementOptions? RiskManagementOptions { get; set; }
    public IIndicatorOptions? IndicatorOptions { get; set; }
    public IStrategyOptions? StrategyOptions { get; set; }
    public IBotOptions? BotOptions { get; set; }
    public IRunnerOptions? RunnerOptions { get; set; }
    public string? FullName { get; set; }
    public string? BrokerName { get; set; }
}
