using System.Collections;

namespace bot.src.Data.Models;

public class Candles : IEnumerable<Candle>
{
    public int TimeFrame { get; private set; } = 0;

    private IEnumerable<Candle> _candles = Array.Empty<Candle>();

    public void SetCandles(IEnumerable<Candle> candles)
    {
        if (!candles.Any())
            throw new ArgumentException("No candle provided.");

        if (candles.Count() >= 2)
            TimeFrame = (int)(candles.ElementAt(0).Date - candles.ElementAt(1).Date).TotalSeconds;
        else
        {
            _candles = candles;
            return;
        }

        Candle firstCandle = candles.First();
        candles = candles.Skip(1);
        _candles = _candles.Append(firstCandle);

        foreach (Candle candle in candles)
            AddCandle(candle);
    }

    public void AddCandle(Candle candle)
    {
        if (!_candles.Any())
        {
            _candles = _candles.Append(candle);
            return;
        }

        if (TimeFrame != 0 && (_candles.Last().Date - candle.Date).TotalSeconds != TimeFrame)
            throw new ArgumentException($"Invalid candle provided.{_candles.Last().Date}");

        _candles = _candles.Append(candle);

        if (TimeFrame == 0 && _candles.Count() >= 2)
            TimeFrame = (int)(_candles.ElementAt(0).Date - _candles.ElementAt(1).Date).TotalSeconds;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<Candle> GetEnumerator()
    {
        foreach (Candle candle in _candles)
            yield return candle;
    }
}
