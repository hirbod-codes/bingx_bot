using Skender.Stock.Indicators;

namespace bot.src.Indicators;

public static class Utils
{
    public static bool HasCrossedOver(IEnumerable<IReusableResult> r1, IEnumerable<IReusableResult> r2) => r1.ElementAt(r1.Count() - 2).Value <= r2.ElementAt(r2.Count() - 2).Value && r1.Last().Value > r2.Last().Value;
    public static bool HasCrossedUnder(IEnumerable<IReusableResult> r1, IEnumerable<IReusableResult> r2) => r1.ElementAt(r1.Count() - 2).Value >= r2.ElementAt(r2.Count() - 2).Value && r1.Last().Value < r2.Last().Value;
}
