using bot.src.Indicators.Models;

namespace bot.src.Indicators.CandlesOpenClose;

public class IndicatorOptions : IIndicatorOptions
{
    public StochasticOptions Stochastic { get; set; } = new() { Source = "close", Period = 14, SignalPeriod = 3, SmoothPeriod = 3 };
}
