using bot.src.Indicators.Models;

namespace bot.src.Indicators.EmaRsi;

public class IndicatorOptions : IIndicatorOptions
{
    public int AtrMultiplier { get; set; } = 2;
    public AtrOptions Atr { get; set; } = null!;
    public EmaOptions Ema1 { get; set; } = null!;
    public EmaOptions Ema2 { get; set; } = null!;
    public RsiOptions Rsi { get; set; } = null!;
}
