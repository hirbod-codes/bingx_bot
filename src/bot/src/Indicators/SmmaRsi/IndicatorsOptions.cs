namespace bot.src.Indicators.SmmaRsi;

public class IndicatorsOptions : IIndicatorsOptions
{
    public SmmaOptions Smma1 { get; set; } = null!;
    public SmmaOptions Smma2 { get; set; } = null!;
    public SmmaOptions Smma3 { get; set; } = null!;
    public RsiOptions Rsi { get; set; } = null!;
}
