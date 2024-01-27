using System.Collections;

namespace bot.src.Data.Models;

public class Candles : IEnumerable<Candle>
{
    private int _timeFrame = 0;
    public int TimeFrame
    {
        get
        {
            if (_timeFrame == 0)
                throw new ZeroTimeFrameException();
            return _timeFrame;
        }
        private set
        {
            _timeFrame = value;
        }
    }

    private IEnumerable<Candle> _candles = Array.Empty<Candle>();

    public void SetCandles(IEnumerable<Candle> candles)
    {
        if (!candles.Any())
            throw new ArgumentException("No candle provided.");

        if (candles.Count() == 1)
        {
            _candles = candles;
            return;
        }

        if (_timeFrame == 0)
            _timeFrame = (int)(candles.ElementAt(0).Date - candles.ElementAt(1).Date).TotalSeconds;

        foreach (Candle candle in candles)
            AddCandle(candle);
    }

    public void AddCandle(Candle candle)
    {
        if (_candles.Count() < 2)
        {
            _candles = _candles.Append(candle);
            return;
        }

        if (_timeFrame == 0)
            _timeFrame = (int)(_candles.ElementAt(0).Date - _candles.ElementAt(1).Date).TotalSeconds;

        double totalSeconds = (_candles.Last().Date - candle.Date).TotalSeconds;

        if (_timeFrame != 0 && (totalSeconds < _timeFrame - 1 || totalSeconds > _timeFrame + 1))
            throw new ArgumentException($"Invalid candle provided.{_candles.Last().Date}");

        _candles = _candles.Append(candle);
        return;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<Candle> GetEnumerator()
    {
        foreach (Candle candle in _candles)
            yield return candle;
    }
}
