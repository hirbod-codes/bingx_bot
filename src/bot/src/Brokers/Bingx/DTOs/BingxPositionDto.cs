namespace bot.src.Brokers.Bingx.DTOs;

public class BingxPositionDto
{
    public string PositionId { get; set; } = null!;
    public string Symbol { get; set; } = null!;
    public string Currency { get; set; } = null!;
    public string PositionAmt { get; set; } = null!;
    public string AvailableAmt { get; set; } = null!;
    public string PositionSide { get; set; } = null!;
    public bool Isolated { get; set; }
    public string AvgPrice { get; set; } = null!;
    public string InitialMargin { get; set; } = null!;
    public string Leverage { get; set; } = null!;
    public string UnrealizedProfit { get; set; } = null!;
    public string RealisedProfit { get; set; } = null!;
    public decimal LiquidationPrice { get; set; }
    public string PnlRatio { get; set; } = null!;
    public string MaxMarginReduction { get; set; } = null!;
    public string RiskRate { get; set; } = null!;
    public string MarkPrice { get; set; } = null!;
    public string PositionValue { get; set; } = null!;
    public bool OnlyOnePosition { get; set; }
}
