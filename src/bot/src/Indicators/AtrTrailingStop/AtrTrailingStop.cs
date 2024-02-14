using System.Collections.ObjectModel;
using bot.src.Data.Models;
using bot.src.Indicators.AtrTrailingStop.Models;
using Skender.Stock.Indicators;

namespace bot.src.Indicators.AtrTrailingStop;

public static class AtrTrailingStop
{
    public static IEnumerable<AtrTrailingStopResult> GetAtrTrailingStop(this Candles candles, int atrPeriod, double atrMultiplier) => candles.ToTupleCollection(CandlePart.Close).CalcAtrTrailingStop(candles.GetAtr(atrPeriod), atrMultiplier);

    public static IEnumerable<AtrTrailingStopResult> CalcAtrTrailingStop(this Collection<(DateTime Date, double Source)> sources, IEnumerable<AtrResult> atr, double atrMultiplier)
    {
        IEnumerable<AtrTrailingStopResult> results = Array.Empty<AtrTrailingStopResult>();
        checked
        {
            for (int i = 0; i < sources.Count; i++)
            {
                AtrTrailingStopResult result = new(sources[i].Date);
                results = results.Append(result);

                double? atrValue = atr.ElementAt(i).Atr;

                if (i == 0 || atrValue == null)
                {
                    result.AtrTrailingStop = 0;
                    continue;
                }

                double multipliedAtr = (double)atrValue! * atrMultiplier;

                double previousAtrTrailingStop = (double)results.ElementAt(i - 1).AtrTrailingStop!;

                if (sources[i].Source > previousAtrTrailingStop && sources[i - 1].Source > previousAtrTrailingStop)
                    result.AtrTrailingStop = Math.Max(previousAtrTrailingStop, sources[i].Source - multipliedAtr);
                else
                {
                    if (sources[i].Source < previousAtrTrailingStop && sources[i - 1].Source < previousAtrTrailingStop)
                        result.AtrTrailingStop = Math.Min(previousAtrTrailingStop, sources[i].Source + multipliedAtr);
                    else
                    {
                        if (sources[i].Source > previousAtrTrailingStop)
                            result.AtrTrailingStop = sources[i].Source - multipliedAtr;
                        else
                            result.AtrTrailingStop = sources[i].Source + multipliedAtr;
                    }
                }
            }
        }

        return results;
    }
}
