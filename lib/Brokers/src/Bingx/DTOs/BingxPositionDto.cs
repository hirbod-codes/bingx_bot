namespace Brokers.src.Bingx.DTOs;

public class BingxPositionDto
{
    public string PositionId { get; set; } = null!;
    public string Symbol { get; set; } = null!;
    public string Currency { get; set; } = null!;
    public decimal PositionAmt { get; set; }
    public decimal AvailableAmt { get; set; }
    public string PositionSide { get; set; } = null!;
    public bool Isolated { get; set; }
    public decimal AvgPrice { get; set; }
    public decimal InitialMargin { get; set; }
    public int Leverage { get; set; }
    public decimal UnrealizedProfit { get; set; }
    public decimal RealisedProfit { get; set; }
    public decimal LiquidationPrice { get; set; }
    public decimal PnlRatio { get; set; }
    public decimal MaxMarginReduction { get; set; }
    public decimal RiskRate { get; set; }
    public decimal MarkPrice { get; set; }
    public decimal PositionValue { get; set; }
    public bool OnlyOnePosition { get; set; }
    public long UpdateTime { get; set; }
}
