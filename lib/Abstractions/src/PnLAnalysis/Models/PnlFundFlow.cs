namespace Abstractions.src.PnLAnalysis.Models;

public class PnlFundFlow
{
    public string Symbol { get; set; } = null!;
    public string IncomeType { get; set; } = null!;
    public string Income { get; set; } = null!;
    public string Asset { get; set; } = null!;
    public string Info { get; set; } = null!;
    public DateTime Time { get; set; }
    public string TranId { get; set; } = null!;
    public string TradeId { get; set; } = null!;
}
