using bot.src.Data.Models;
using bot.src.Indicators.Models;

namespace bot.src.Indicators;

public static class Sma
{
    public static IEnumerable<SmaValue> GetDecimals(this Candles candles, int length)
    {
        IEnumerable<SmaValue> smaValues = Array.Empty<SmaValue>();

        for (int i = candles.Count() - 1; i > -1; i--)
        {
            Candle candle = candles.ElementAt(i);
            if (i + length - 1 >= candles.Count())
            {
                smaValues = smaValues.Append(new() { Date = candle.Date, Value = null });
                continue;
            }

            decimal sma = 0;
            for (int j = 0; j < length - 1; j++)
                sma += candles.ElementAt(i + j).Close / length;

            smaValues = smaValues.Append(new() { Date = candle.Date, Value = sma });
        }

        return smaValues;
    }
}
