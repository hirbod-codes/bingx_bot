using bot.src.Indicators.Models;
using Skender.Stock.Indicators;

namespace bot.src.Indicators;

public static class Indicators
{
    public static IEnumerable<SuperTrendV1Result> GetSuperTrendV1(this IEnumerable<IQuote> quotes, int lookBackPeriods = 10, CandlePart candlePart = CandlePart.HL2, decimal multiplier = 2.0m, bool changeATRCalculationMethod = true) =>
        quotes.CalcSuperTrendV1(lookBackPeriods, (double)multiplier, changeATRCalculationMethod, candlePart);

    public static IEnumerable<SuperTrendResult> GetSuperTrend(this IEnumerable<IQuote> quotes, int lookBackPeriods = 10, CandlePart candlePart = CandlePart.HL2, decimal multiplier = 2.0m, bool changeATRCalculationMethod = true) =>
        quotes.CalcSuperTrend(lookBackPeriods, (double)multiplier, changeATRCalculationMethod, candlePart);

    public static List<SuperTrendResult> CalcSuperTrend(this IEnumerable<IQuote> quotes, int lookBackPeriods, double multiplier, bool changeATRCalculationMethod, CandlePart candlePart)
    {
        // check parameter arguments
        ValidateSuperTrend(lookBackPeriods, multiplier);

        List<IQuote> quotesList = quotes.ToList();

        // initialize
        List<SuperTrendResult> results = new(quotesList.Count);
        IEnumerable<AtrResult> atrResults = quotesList.GetAtr(lookBackPeriods);

        bool isBullish = true;
        double? upperBand = null;
        double? lowerBand = null;

        // roll through quotes
        for (int i = 0; i < quotesList.Count; i++)
        {
            IQuote q = quotesList[i];

            SuperTrendResult r = new(q.Date);
            results.Add(r);

            if (i < lookBackPeriods)
                continue;

            double? mid = (double?)Utils.GetCandlePart(candlePart, q);
            double? atr = atrResults.ElementAt(i).Atr;
            double? prevClose = (double?)quotesList[i - 1].Close;

            // potential bands
            double? upperEval = mid + (multiplier * atr);
            double? lowerEval = mid - (multiplier * atr);

            // initial values
            if (i == lookBackPeriods)
            {
                isBullish = (double)q.Close >= mid;

                upperBand = upperEval;
                lowerBand = lowerEval;
            }

            if (upperEval < upperBand || prevClose > upperBand)
                upperBand = upperEval;

            if (lowerEval > lowerBand || prevClose < lowerBand)
                lowerBand = lowerEval;

            if ((double)q.Close <= (isBullish ? lowerBand : upperBand))
            {
                r.SuperTrend = (decimal?)upperBand;
                r.UpperBand = (decimal?)upperBand;
                isBullish = false;
            }
            else
            {
                r.SuperTrend = (decimal?)lowerBand;
                r.LowerBand = (decimal?)lowerBand;
                isBullish = true;
            }
        }

        return results;
    }

    private static void ValidateSuperTrend(int lookBackPeriods, double multiplier)
    {
        if (lookBackPeriods <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(lookBackPeriods), lookBackPeriods, "look Back periods must be greater than 1 for SuperTrend.");
        }

        if (multiplier <= 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(multiplier), multiplier, "Multiplier must be greater than 0 for SuperTrend.");
        }
    }

    public static List<SuperTrendV1Result> CalcSuperTrendV1(this IEnumerable<IQuote> quotes, int lookBackPeriods, double multiplier, bool changeATRCalculationMethod, CandlePart candlePart)
    {
        // check parameter arguments
        ValidateSuperTrend(lookBackPeriods, multiplier);

        List<IQuote> quotesList = quotes.ToList();

        // initialize
        List<SuperTrendV1Result> results = new(quotesList.Count);
        IEnumerable<AtrResult> atrResults = quotesList.GetAtr(lookBackPeriods);

        bool isBullish = true;
        double? upperBand = null;
        double? lowerBand = null;

        // roll through quotes
        for (int i = 0; i < quotesList.Count; i++)
        {
            IQuote q = quotesList[i];

            SuperTrendV1Result r = new(q.Date);
            results.Add(r);

            if (i < lookBackPeriods)
                continue;

            double? mid = (double?)Utils.GetCandlePart(candlePart, q);
            double? atr = atrResults.ElementAt(i).Atr;
            double? prevClose = (double?)quotesList[i - 1].Close;

            // potential bands
            double? upperEval = mid + (multiplier * atr);
            double? lowerEval = mid - (multiplier * atr);

            // initial values
            if (i == lookBackPeriods)
            {
                isBullish = (double)q.Close >= mid;

                upperBand = upperEval;
                lowerBand = lowerEval;
            }

            if (upperEval < upperBand || prevClose > upperBand)
                upperBand = upperEval;

            if (lowerEval > lowerBand || prevClose < lowerBand)
                lowerBand = lowerEval;

            bool previousIsBullish = isBullish;

            if ((double)q.Close <= (isBullish ? lowerBand : upperBand))
            {
                r.SuperTrend = (decimal?)upperBand;
                r.UpperBand = (decimal?)upperBand;
                isBullish = false;
            }
            else
            {
                r.SuperTrend = (decimal?)lowerBand;
                r.LowerBand = (decimal?)lowerBand;
                isBullish = true;
            }

            if (previousIsBullish && !isBullish)
                r.SellSignal = true;
            else if (!previousIsBullish && isBullish)
                r.BuySignal = true;
        }

        return results;
    }

    private static void ValidateSuperTrendV1(int lookBackPeriods, double multiplier)
    {
        if (lookBackPeriods <= 1)
            throw new ArgumentOutOfRangeException(nameof(lookBackPeriods), lookBackPeriods, "Look back periods must be greater than 1 for SuperTrend.");

        if (multiplier <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(multiplier), multiplier, "Multiplier must be greater than 0 for SuperTrend.");
    }
}
