namespace bot.src.Runners.StochasticEma;

public class RunnerOptions : IRunnerOptions
{
    public int TimeFrame { get; set; }
    public int HistoricalCandlesCount { get; set; } = 30000;
}
