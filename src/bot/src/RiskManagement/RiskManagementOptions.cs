namespace bot.src.RiskManagement;

public record class RiskManagementOptions
{
    public decimal Margin { get; set; }
    public decimal Leverage { get; set; }
    public decimal Commission { get; set; }
    public decimal Ratio { get; set; }
    public decimal SLPercentages { get; set; }
}
