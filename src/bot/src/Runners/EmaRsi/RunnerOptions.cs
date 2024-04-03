namespace bot.src.Runners.EmaRsi;

public class RunnerOptions : IRunnerOptions
{
    public int TimeFrame { get; set; }
    public int HistoricalCandlesCount { get; set; } = 30000;
}
