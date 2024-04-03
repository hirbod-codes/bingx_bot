namespace bot.src.RiskManagement.EmaStochasticSuperTrend;

public record class RiskManagementOptions : IRiskManagementOptions
{
    public decimal Margin { get; set; }
    public decimal SLPercentages { get; set; }
    public decimal RiskRewardRatio { get; set; }
    public decimal BrokerCommission { get; set; }
    public decimal CommissionPercentage { get; set; }
    /// <summary>
    /// if zero, no limit will be applied on the number concurrent positions.
    /// </summary>
    public decimal NumberOfConcurrentPositions { get; set; }
    public decimal GrossLossLimit { get; set; }
    public decimal GrossProfitLimit { get; set; }
}
