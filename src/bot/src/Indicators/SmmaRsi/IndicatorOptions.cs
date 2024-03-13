using bot.src.Indicators.Models;

namespace bot.src.Indicators.SmmaRsi;

public class IndicatorOptions : IIndicatorOptions
{
    public double AtrMultiplier { get; set; } = 2;
    public AtrOptions Atr { get; set; } = null!;
    public SmmaOptions Smma1 { get; set; } = null!;
    public SmmaOptions Smma2 { get; set; } = null!;
    public SmmaOptions Smma3 { get; set; } = null!;
    public RsiOptions Rsi { get; set; } = null!;
}
