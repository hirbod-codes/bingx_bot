namespace bot.src.Bots.SuperTrendV1;

public class BotOptions : IBotOptions
{
    public string Provider { get; set; } = null!;
    public int TimeFrame { get; set; }
    public bool ShouldSkipOnParallelPositionRequest { get; internal set; } = false;
    public int RetryCount { get; internal set; } = 3;
}
