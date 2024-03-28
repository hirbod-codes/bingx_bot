using bot.src.Indicators.Models;

namespace bot.src.Indicators.EmaStochasticSuperTrend;

public class IndicatorOptions : IIndicatorOptions
{
    public AtrOptions Atr { get; set; } = new() { Period = 14, Source = "close" };
    public double AtrMultiplier { get; set; } = 2;
    public EmaOptions Ema1 { get; set; } = new() { Period = 10, Source = "close" };
    public EmaOptions Ema2 { get; set; } = new() { Period = 50, Source = "close" };
    public StochasticOptions Stochastic { get; set; } = new() { Source = "close", Period = 14, SignalPeriod = 3, SmoothPeriod = 3 };
    public SuperTrendOptions SuperTrend { get; set; } = new() { Period = 20, Multiplier = 2 };
}