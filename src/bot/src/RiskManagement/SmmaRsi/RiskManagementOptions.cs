namespace bot.src.RiskManagement.SmmaRsi;

public record class RiskManagementOptions : IRiskManagementOptions
{
    public decimal Margin { get; set; }
    public decimal SLPercentages { get; set; }
    /// <summary>
    /// if zero, no limit will be applied on the number concurrent positions.
    /// </summary>
    public decimal NumberOfConcurrentPositions { get; set; }
    public decimal GrossLossLimit { get; set; }
    /// <summary>
    /// If zero, no limit will be applied, otherwise it's the time period that risk management considers when it checks for gross loss limit (in seconds).
    /// </summary>
    public decimal GrossLossInterval { get; set; }
    public decimal GrossProfitLimit { get; set; }
    /// <summary>
    /// If zero, no limit will be applied, otherwise it's the time period that risk management considers when it checks for gross profit limit (in seconds).
    /// </summary>
    public decimal GrossProfitInterval { get; set; }
}
