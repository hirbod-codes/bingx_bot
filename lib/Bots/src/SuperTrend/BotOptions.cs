using Abstractions.src.Bots;

namespace Bots.src.SuperTrendV1;

public class BotOptions : IBotOptions
{
    public string Provider { get; set; } = null!;
    public int TimeFrame { get; set; } = 3600;
    public bool ShouldSkipOnParallelPositionRequest { get; set; } = false;
    public int RetryCount { get; set; } = 3;
}
