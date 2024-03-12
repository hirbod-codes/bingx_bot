using bot.src.Indicators.Models;

namespace bot.src.Indicators.StochasticEma;

public class IndicatorOptions : IIndicatorOptions
{
    public EmaOptions Ema { get; set; } = null!;
    public StochasticOptions Stochastic { get; set; } = null!;
    public double StochasticUpperBand { get; set; } = 80;
    public double StochasticLowerBand { get; set; } = 20;
    public AtrOptions Atr { get; set; } = null!;
    public double AtrMultiplier { get; set; } = 2;
}
