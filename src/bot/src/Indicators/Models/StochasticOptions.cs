namespace bot.src.Indicators.StochasticEma;

public class StochasticOptions
{
    public int Period { get; set; }
    public int SignalPeriod { get; set; }
    public int SmoothPeriod { get; set; }
    public string Source { get; set; } = null!;
}
