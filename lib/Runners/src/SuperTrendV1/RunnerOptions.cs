using Abstractions.src.Runners;

namespace Runners.src.SuperTrendV1;

public class RunnerOptions : IRunnerOptions
{
    public int TimeFrame { get; set; }
    public int HistoricalCandlesCount { get; set; } = 5000;
}
