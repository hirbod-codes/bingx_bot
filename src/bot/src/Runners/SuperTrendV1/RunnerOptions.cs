namespace bot.src.Runners.SuperTrendV1;

public class RunnerOptions : IRunnerOptions
{
    public int TimeFrame { get; set; }
    public int HistoricalCandlesCount { get; set; } = 5000;
}
