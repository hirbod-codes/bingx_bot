using bot.src.Indicators.Models;

namespace bot.src.Indicators.Luck;

public class IndicatorOptions : IIndicatorOptions
{
    public AtrOptions Atr { get; set; } = null!;
    public double AtrMultiplier { get; set; } = 2;
}
