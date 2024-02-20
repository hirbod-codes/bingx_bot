using bot.src.PnLAnalysis.Models;
using bot.src.Data.Models;
using bot.src.MessageStores;
using bot.src.Brokers;
using bot.src.Bots;
using bot.src.RiskManagement;
using bot.src.Indicators;
using bot.src.Strategies;
using StrategyTester.src.Testers;

namespace StrategyTester.Dtos;

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
