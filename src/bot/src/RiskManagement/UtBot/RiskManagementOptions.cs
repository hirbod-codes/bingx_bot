namespace bot.src.RiskManagement.UtBot;

public record class RiskManagementOptions : IRiskManagementOptions
{
    public decimal Margin { get; set; }
    public decimal SLPercentages { get; set; }
}
