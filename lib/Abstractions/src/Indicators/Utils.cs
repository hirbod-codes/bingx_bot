using Skender.Stock.Indicators;
using CandlePart = Abstractions.src.Indicators.Models.CandlePart;

namespace Abstractions.src.Indicators;

public static class Utils
{
    public static decimal GetCandlePart(CandlePart candlePart, IQuote candle) => candlePart switch
    {
        CandlePart.Open => candle.Open,
        CandlePart.High => candle.High,
        CandlePart.Low => candle.Low,
        CandlePart.Close => candle.Close,
        CandlePart.Volume => candle.Volume,
        CandlePart.HL2 => (candle.High + candle.Low) / 2.0m,
        CandlePart.HLC3 => (candle.High + candle.Low + candle.Close) / 3.0m,
        CandlePart.OC2 => (candle.Open + candle.Close) / 2.0m,
        CandlePart.OHL3 => (candle.Open + candle.High + candle.Low) / 3.0m,
        CandlePart.OHLC4 => (candle.Open + candle.High + candle.Low + candle.Close) / 4.0m,
        CandlePart.HLCC4 => (candle.High + candle.Low + candle.Close + candle.Close) / 4.0m,
        _ => throw new NotImplementedException()
    };

    public static bool HasCrossedOver(IEnumerable<IReusableResult> r1, IEnumerable<IReusableResult> r2) => r1.ElementAt(r1.Count() - 2).Value <= r2.ElementAt(r2.Count() - 2).Value && r1.Last().Value > r2.Last().Value;

    public static bool HasCrossedUnder(IEnumerable<IReusableResult> r1, IEnumerable<IReusableResult> r2) => r1.ElementAt(r1.Count() - 2).Value >= r2.ElementAt(r2.Count() - 2).Value && r1.Last().Value < r2.Last().Value;
}
