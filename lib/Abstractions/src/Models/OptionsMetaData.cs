using Abstractions.src.RiskManagement;
using Abstractions.src.Brokers;
using Abstractions.src.Bots;
using Abstractions.src.Runners;
using Abstractions.src.Indicators;
using Abstractions.src.Strategies;
using Abstractions.src.MessageStores;

namespace Abstractions.src.Models;

public static class OptionsMetaData
{
    public static string? PositionRepositoryName { get; set; }
    public static string? MessageRepositoryName { get; set; }
    public static string? NotifierName { get; set; }
    public static string? MessageStoreName { get; set; }
    public static string? BrokerName { get; set; }
    public static string? RiskManagementName { get; set; }
    public static string? IndicatorOptionsName { get; set; }
    public static string? StrategyName { get; set; }
    public static string? BotName { get; set; }
    public static string? RunnerName { get; set; }
}
