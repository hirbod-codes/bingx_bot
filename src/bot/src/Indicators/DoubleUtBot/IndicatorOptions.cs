using bot.src.Indicators.Models;

namespace bot.src.Indicators.DoubleUtBot;

public class IndicatorOptions : IIndicatorOptions
{
    public EmaOptions EmaPeriod1 { get; set; } = null!;
    public AtrOptions AtrPeriod1 { get; set; } = null!;
    public double AtrMultiplier1 { get; set; }

    public EmaOptions EmaPeriod2 { get; set; } = null!;
    public AtrOptions AtrPeriod2 { get; set; } = null!;
    public double AtrMultiplier2 { get; set; }
}
