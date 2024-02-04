using bot.src.Data.Models;
using Skender.Stock.Indicators;

namespace bot.src.Indicators.Calculators;

public static class Smma
{
    public static IEnumerable<SmmaResult> AddSmmaResult(this IEnumerable<SmmaResult> results, Candle newCandle, int lookBackPeriods)
    {
        double? smma = (results.ElementAt(0).Smma * (lookBackPeriods - 1) + (double)newCandle.Close) / lookBackPeriods;

        results = results.Append(new SmmaResult(newCandle.Date)
        {
            Smma = smma.NaN2Null()
        });

        return results;
    }
}
