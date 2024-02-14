using Skender.Stock.Indicators;

namespace bot.src.Indicators.AtrTrailingStop.Models;

public sealed class AtrTrailingStopResult : ResultBase, IReusableResult, ISeries
{
    public double? AtrTrailingStop { get; set; }

    double? IReusableResult.Value => AtrTrailingStop;

    public AtrTrailingStopResult(DateTime date)
    {
        base.Date = date;
    }
}
