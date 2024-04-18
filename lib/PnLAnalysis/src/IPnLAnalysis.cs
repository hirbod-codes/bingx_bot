using Abstractions.src.Models;
using PnLAnalysis.src.Models;

namespace PnLAnalysis.src;

public interface IPnLAnalysis
{
    public AnalysisSummary RunAnalysis(IEnumerable<Position> closedPositions);
    public decimal GetNetProfit(IEnumerable<Position> closedPositions);
    public decimal GetLongNetProfit(IEnumerable<Position> closedPositions);
    public decimal GetShortNetProfit(IEnumerable<Position> closedPositions);
    public decimal GetGrossProfit(IEnumerable<Position> closedPositions);
    public decimal GetGrossLoss(IEnumerable<Position> closedPositions);
    public decimal GetLongGrossProfit(IEnumerable<Position> closedPositions);
    public decimal GetShortGrossProfit(IEnumerable<Position> closedPositions);
    public decimal GetLongGrossLoss(IEnumerable<Position> closedPositions);
    public decimal GetShortGrossLoss(IEnumerable<Position> closedPositions);
}
