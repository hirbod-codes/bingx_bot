namespace bot.src.Runners.DoubleUtBot;

public class RunnerOptions : IRunnerOptions
{
    public int TimeFrame { get; set; }
    public int HistoricalCandlesCount { get; set; } = 30000;
}
