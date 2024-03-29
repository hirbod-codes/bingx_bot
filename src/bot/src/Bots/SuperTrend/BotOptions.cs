namespace bot.src.Bots.SuperTrend;

public class BotOptions : IBotOptions
{
    public string Provider { get; set; } = null!;
    public int TimeFrame { get; set; }
    public bool ShouldDivideMargin { get; set; } = false;
}
