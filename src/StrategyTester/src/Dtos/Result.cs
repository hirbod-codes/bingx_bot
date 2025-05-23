using Abstractions.src.PnLAnalysis.Models;
using Abstractions.src.Data.Models;

namespace StrategyTester.src.Dtos;

public class Result
{
    public object MessageStoreOptions { get; set; } = null!;
    public object BrokerOptions { get; set; } = null!;
    public object BotOptions { get; set; } = null!;
    public object RiskManagementOptions { get; set; } = null!;
    public object IndicatorsOptions { get; set; } = null!;
    public object StrategyOptions { get; set; } = null!;
    public object TesterOptions { get; set; } = null!;
    public IEnumerable<Position> ClosedPositions { get; set; } = null!;
    public AnalysisSummary PnlResults { get; set; } = null!;
}
