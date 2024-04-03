using bot.src.Strategies;

namespace bot.src.Bots.SuperTrendV1;

public class BotOptions : IBotOptions
{
    public string Provider { get; set; } = StrategyNames.SUPER_TREND_V1;
    public int TimeFrame { get; set; } = 3600;
    public bool ShouldSkipOnParallelPositionRequest { get; internal set; } = false;
    public int RetryCount { get; internal set; } = 3;
}
