namespace bot.src.RiskManagement.SmmaRsi;

public record class RiskManagementOptions : IRiskManagementOptions
{
    public decimal Margin { get; set; }
    public decimal Leverage { get; set; }
    public decimal Ratio { get; set; }
    public decimal SLPercentages { get; set; }
}
