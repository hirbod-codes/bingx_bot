
namespace Abstractions.src.PnLAnalysis.Models;

public class AnalysisSummary
{
    public decimal NetProfitPercentage { get; set; }
    public decimal HighestNetProfitPercentage { get; set; }
    public decimal HighestDrawDownPercentage { get; set; }
    public decimal BuyAndHoldProfitPercentage { get; set; }
    public decimal BuyAndHoldProfit { get; set; }
    public decimal HighestNetProfit { get; set; }
    public decimal NetProfit { get; set; }
    public decimal HighestDrawDown { get; set; }
    public decimal DrawDown { get; set; }
    public decimal LongGrossProfit { get; set; }
    public decimal ShortGrossProfit { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal LongGrossLoss { get; set; }
    public decimal ShortGrossLoss { get; set; }
    public decimal GrossLoss { get; set; }
    public int SignalsCount { get; set; } = 0;
    public int ShortPositionCount { get; set; } = 0;
    public int LongPositionCount { get; set; } = 0;
    public int OpenedPositionsCount { get; set; } = 0;
    public int PendingPositionsCount { get; set; } = 0;
    public int CancelledPositionsCount { get; set; } = 0;
    public int ClosedPositionsCount { get; set; } = 0;
    public int Wins { get; set; } = 0;
    public int Losses { get; set; } = 0;
    public int UnknownStatePositions { get; set; } = 0;
    public int UnacceptableOrdersCount { get; set; } = 0;
    public int LongWins { get; set; } = 0;
    public int ShortWins { get; set; } = 0;
    public int LongLosses { get; set; } = 0;
    public int ShortLosses { get; set; } = 0;
    public decimal WinLossRatio { get; set; } = 0;
    public Dictionary<string, object> Indicators { get; set; } = new();
}
