using bot.src.Bots;
using bot.src.Brokers;
using bot.src.Indicators;
using bot.src.RiskManagement;
using bot.src.Runners;
using bot.src.Strategies;

namespace bot.src.Models;

public static class OptionsNames
{
    public static string? PositionRepositoryName { get; set; }
    public static string? MessageRepositoryName { get; set; }
    public static string? NotifierName { get; set; }
    public static string? MessageStoreName { get; set; }
    public static string? _brokerName;
    public static string? BrokerName { get { return _brokerName; } set { _brokerName = value; BrokerOptionsType = BrokerOptionsFactory.GetInstanceType(value); } }
    public static Type? BrokerOptionsType { get; set; }
    public static string? _riskManagementName;
    public static string? RiskManagementName { get { return _riskManagementName; } set { _riskManagementName = value; RiskManagementOptionsType = RiskManagementOptionsFactory.GetInstanceType(value); } }
    public static Type? RiskManagementOptionsType { get; set; }
    public static string? _indicatorOptionsName;
    public static string? IndicatorOptionsName { get { return _indicatorOptionsName; } set { _indicatorOptionsName = value; IndicatorOptionsType = IndicatorOptionsFactory.GetInstanceType(value); } }
    public static Type? IndicatorOptionsType { get; set; }
    public static string? _strategyName;
    public static string? StrategyName { get { return _strategyName; } set { _strategyName = value; StrategyOptionsType = StrategyOptionsFactory.GetInstanceType(value); } }
    public static Type? StrategyOptionsType { get; set; }
    public static string? _botName;
    public static string? BotName { get { return _botName; } set { _botName = value; BotOptionsType = BotOptionsFactory.GetInstanceType(value); } }
    public static Type? BotOptionsType { get; set; }
    public static string? _runnerName;
    public static string? RunnerName { get { return _runnerName; } set { _runnerName = value; RunnerOptionsType = RunnerOptionsFactory.GetInstanceType(value); } }
    public static Type? RunnerOptionsType { get; set; }
}
