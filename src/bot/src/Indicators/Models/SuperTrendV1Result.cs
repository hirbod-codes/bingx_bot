using Skender.Stock.Indicators;

namespace bot.src.Indicators.Models;

public class SuperTrendV1Result : ResultBase
{
    public decimal? SuperTrend { get; set; }

    public decimal? UpperBand { get; set; }

    public decimal? LowerBand { get; set; }

    public bool BuySignal { get; set; } = false;

    public bool SellSignal { get; set; } = false;

    public SuperTrendV1Result(DateTime date) => base.Date = date;
}