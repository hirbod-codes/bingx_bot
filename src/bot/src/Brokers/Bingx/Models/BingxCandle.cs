namespace bot.src.Brokers.Bingx.Models;

public class BingxCandle
{
    public string Open { get; set; } = null!;
    public string Close { get; set; } = null!;
    public string High { get; set; } = null!;
    public string Low { get; set; } = null!;
    public string Volume { get; set; } = null!;
    public long Time { get; set; }
}
