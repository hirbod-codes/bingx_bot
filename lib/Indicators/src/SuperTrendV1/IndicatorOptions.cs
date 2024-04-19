using Abstractions.src.Indicators;
using Abstractions.src.Indicators.Models;

namespace Indicators.src.SuperTrendV1;

public class IndicatorOptions : IIndicatorOptions
{
    public AtrOptions Atr { get; set; } = new() { Period = 14, Source = "close" };
    public double AtrMultiplier { get; set; } = 2;
    public SuperTrendV1Options SuperTrendOptions { get; set; } = new() { Multiplier = 3.2, Period = 30, CandlePart = CandlePart.HLCC4, ChangeATRCalculationMethod = true };
}
