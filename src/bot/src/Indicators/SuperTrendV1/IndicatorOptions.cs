using bot.src.Indicators.Models;

namespace bot.src.Indicators.SuperTrendV1;

public class IndicatorOptions : IIndicatorOptions
{
    public AtrOptions Atr { get; set; } = new() { Period = 14, Source = "close" };
    public double AtrMultiplier { get; set; } = 4;
    public SuperTrendV1Options SuperTrendOptions { get; set; } = new() { Multiplier = 1.5, Period = 30, CandlePart = CandlePart.HLCC4, ChangeATRCalculationMethod = true };
}
