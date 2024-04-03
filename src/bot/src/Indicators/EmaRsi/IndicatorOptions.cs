using bot.src.Indicators.Models;

namespace bot.src.Indicators.EmaRsi;

public class IndicatorOptions : IIndicatorOptions
{
    public double AtrMultiplier { get; set; } = 2;
    public AtrOptions Atr { get; set; } = new() { Period = 14, Source = "close" };
    public EmaOptions Ema1 { get; set; } = new() { Period = 20, Source = "close" };
    public EmaOptions Ema2 { get; set; } = new() { Period = 30, Source = "close" };
    public RsiOptions Rsi { get; set; } = new() { Period = 14, Source = "close", UpperBand = 54, LowerBand = 46 };
}
