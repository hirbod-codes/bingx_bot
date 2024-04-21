namespace Brokers.src.Bingx.Models;

public class BingxPnlFundFlow
{
    public string Symbol { get; set; } = null!;
    public string IncomeType { get; set; } = null!;
    public string Income { get; set; } = null!;
    public string Asset { get; set; } = null!;
    public string Info { get; set; } = null!;
    public long Time { get; set; }
    public string TranId { get; set; } = null!;
    public string TradeId { get; set; } = null!;
}
