using bot.src.Data.Models;
using Skender.Stock.Indicators;

namespace bot.src.Indicators.Calculators;

public static class Smma
{
    public static IEnumerable<SmmaResult> AddSmmaResult(this IEnumerable<SmmaResult> smmaResults, Candle newCandle, int lookBackPeriods)
    {
        double? smma = (smmaResults.ElementAt(0).Smma * (lookBackPeriods - 1) + (double)newCandle.Close) / lookBackPeriods;

        smmaResults = smmaResults.Append(new SmmaResult(newCandle.Date)
        {
            Smma = smma.NaN2Null()
        });

        return smmaResults;
    }
}

public static class UtBot
{
    public static IEnumerable<UtBotResult> AddUtBotResult(this IEnumerable<UtBotResult> utBotResults, IEnumerable<AtrResult> atrResults, IEnumerable<EmaResult> emaResults, Candle newCandle, Candle previousCandle, int lookBackPeriods)
    {
        decimal? atrTrailingStop = 0;

        utBotResults = utBotResults.Append(new UtBotResult(newCandle.Date)
        {
        });

        return utBotResults;
    }
}

public static class Ema
{
    public static IEnumerable<EmaResult> AddUtBotResult(this IEnumerable<EmaResult> emaResults, Candle newCandle, Candle previousCandle, int lookBackPeriods)
    {
        decimal? atrTrailingStop = 0;

        emaResults = emaResults.Append(new EmaResult(newCandle.Date)
        {
        });

        return emaResults;
    }
}

public static class Atr
{
    public static IEnumerable<AtrResult> AddUtBotResult(this IEnumerable<AtrResult> atrResults, Candle newCandle, Candle previousCandle, int lookBackPeriods)
    {
        decimal? atrTrailingStop = 0;

        atrResults = atrResults.Append(new AtrResult(newCandle.Date)
        {
        });

        return atrResults;
    }
}

public class UtBotResult : ResultBase, IReusableResult, ISeries
{
    public double? Atr { get; set; }
 
    public double? Ema { get; set; }

    public double? AtrTrailingStop { get; set; }

    /// <summary>
    /// 0 for short, 1 for long and -1 for no alert
    /// </summary>
    /// <value></value>
    public double? UtBotAlert { get; set; }

    double? IReusableResult.Value => UtBotAlert;

    public UtBotResult(DateTime date)
    {
        base.Date = date;
    }
}
