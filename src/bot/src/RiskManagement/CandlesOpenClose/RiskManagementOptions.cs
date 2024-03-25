namespace bot.src.RiskManagement.CandlesOpenClose;

public record class RiskManagementOptions : IRiskManagementOptions
{
    public decimal Margin { get; set; } = 100;
    public decimal SLPercentages { get; set; } = 2;
    public decimal RiskRewardRatio { get; set; } = 2;
    public decimal BrokerCommission { get; set; } = 0;
    public decimal CommissionPercentage { get; set; } = 10;
    /// <summary>
    /// if zero, no limit will be applied on the number concurrent positions.
    /// </summary>
    public decimal NumberOfConcurrentPositions { get; set; } = 0;
    public decimal GrossLossLimit { get; set; } = 0;
    public decimal GrossProfitLimit { get; set; } = 0;
}
