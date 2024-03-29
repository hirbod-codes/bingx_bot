using bot.src.Indicators.Models;

namespace bot.src.Indicators.SuperTrendV1;

public class IndicatorOptions : IIndicatorOptions
{
    public AtrOptions Atr { get; set; } = null!;
    public double AtrMultiplier { get; set; } = 2;
    public SuperTrendV1Options SuperTrendOptions { get; set; } = new() { Multiplier = 3.2, Period = 30, CandlePart = CandlePart.HLCC4, ChangeATRCalculationMethod = true };
}
