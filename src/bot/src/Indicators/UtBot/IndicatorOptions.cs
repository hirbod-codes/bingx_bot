using bot.src.Indicators.Models;

namespace bot.src.Indicators.UtBot;

public class IndicatorOptions : IIndicatorOptions
{
    public EmaOptions EmaPeriod { get; set; } = null!;
    public AtrOptions AtrPeriod { get; set; } = null!;
    public double AtrMultiplier { get; set; }
}
