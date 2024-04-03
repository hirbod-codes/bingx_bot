namespace StrategyTester.src.Testers.General;

public class TesterOptions : ITesterOptions
{
    public int TimeFrame { get; set; } = 60;
    public int? CandlesCount { get; set; } = null;
}
