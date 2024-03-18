using bot.src.Indicators.Models;

namespace bot.src.Indicators.EmaStochasticSuperTrend;

public class IndicatorOptions : IIndicatorOptions
{
    public AtrOptions Atr { get; set; } = new() { Period = 14, Source = "close" };
    public double AtrMultiplier { get; set; } = 2;
    public EmaOptions Ema1 { get; set; } = new() { Period = 10, Source = "close" };
    public EmaOptions Ema2 { get; set; } = new() { Period = 10, Source = "close" };
    public EmaOptions Ema3 { get; set; } = new() { Period = 10, Source = "close" };
    public EmaOptions Ema4 { get; set; } = new() { Period = 10, Source = "close" };
    public EmaOptions Ema5 { get; set; } = new() { Period = 10, Source = "close" };
    public EmaOptions Ema6 { get; set; } = new() { Period = 10, Source = "close" };
    public EmaOptions Ema7 { get; set; } = new() { Period = 10, Source = "close" };
    public StochasticOptions Stochastic { get; set; } = new() { Source = "close", Period = 14, SignalPeriod = 3, SmoothPeriod = 3 };
    public SuperTrendOptions SuperTrend { get; set; } = new() { Period = 10, Multiplier = 2 };
}
