using bot.src.Indicators.Models;

namespace bot.src.Indicators.SuperTrend;

public class IndicatorOptions : IIndicatorOptions
{
    public AtrOptions Atr { get; set; } = new() { Period = 14, Source = "close" };
    public double AtrMultiplier { get; set; } = 2;
    public SuperTrendOptions SuperTrendOptions { get; set; } = new() { Multiplier = 2, Period = 10 };
}
