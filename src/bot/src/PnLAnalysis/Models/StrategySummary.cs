namespace bot.src.PnLAnalysis.Models;

public class AnalysisSummary
{
    public decimal HighestNetProfit { get; set; }
    public decimal HighestDrawDown { get; set; }
    public decimal DrawDown { get; set; }
    public decimal NetProfit { get; set; }
    public decimal LongGrossProfit { get; set; }
    public decimal ShortGrossProfit { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal LongGrossLoss { get; set; }
    public decimal ShortGrossLoss { get; set; }
    public decimal GrossLoss { get; set; }
    public int ShortPositionCount { get; set; } = 0;
    public int LongPositionCount { get; set; } = 0;
    public int OpenedPositions { get; set; } = 0;
    public int PendingPositions { get; set; } = 0;
    public int CancelledPositions { get; set; } = 0;
    public int ClosedPositions { get; set; } = 0;
    public int LongWins { get; set; } = 0;
    public int ShortWins { get; set; } = 0;
    public int Wins { get; set; } = 0;
    public int LongLosses { get; set; } = 0;
    public int ShortLosses { get; set; } = 0;
    public int Losses { get; set; } = 0;
    public decimal WinLossRatio { get; set; } = 0;
}
