namespace Brokers.src.Bingx.Models;

public class BingxBalance
{
    public decimal UserId { get; set; }
    public string Asset { get; set; } = null!;
    public decimal Balance { get; set; }
    public decimal Equity { get; set; }
    public decimal UnrealizedProfit { get; set; }
    public decimal RealisedProfit { get; set; }
    public decimal AvailableMargin { get; set; }
    public decimal UsedMargin { get; set; }
    public decimal FreezedMargin { get; set; }
    public decimal ShortUid { get; set; }
}
