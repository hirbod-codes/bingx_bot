namespace Abstractions.src.Models;

public class OptionsMetaData
{
    public string? CandleRepositoryName { get; set; }
    public string? PositionRepositoryName { get; set; }
    public string? MessageRepositoryName { get; set; }
    public string? MessageStoreName { get; set; }
    public string? NotifierName { get; set; }
    public string? BrokerName { get; set; }
    public string? RiskManagementName { get; set; }
    public string? IndicatorOptionsName { get; set; }
    public string? StrategyName { get; set; }
    public string? BotName { get; set; }
    public string? RunnerName { get; set; }

    public OptionsMetaData(string? candleRepositoryName, string? positionRepositoryName, string? messageRepositoryName, string? notifierName, string? messageStoreName, string? brokerName, string? riskManagementName, string? strategyName, string? indicatorOptionsName, string? botName, string? runnerName)
    {
        CandleRepositoryName = candleRepositoryName;
        PositionRepositoryName = positionRepositoryName;
        MessageRepositoryName = messageRepositoryName;
        NotifierName = notifierName;
        MessageStoreName = messageStoreName;
        BrokerName = brokerName;
        RiskManagementName = riskManagementName;
        IndicatorOptionsName = strategyName;
        StrategyName = indicatorOptionsName;
        BotName = botName;
        RunnerName = runnerName;
    }
}
