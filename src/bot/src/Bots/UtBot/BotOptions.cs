namespace bot.src.Bots.UtBot;

public class BotOptions : IBotOptions
{
    public string Provider { get; set; } = null!;
    public int TimeFrame { get; set; }
}
