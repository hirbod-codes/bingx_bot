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
        _candles = Array.Empty<Candle>();

        if (!candles.Any())
            throw new ArgumentException("No candle provided.");

        if (candles.Count() == 1)
        {
            _candles = candles;
            return;
        }

        if (_timeFrame == 0)
            _timeFrame = Math.Abs((int)(candles.ElementAt(0).Date - candles.ElementAt(1).Date).TotalSeconds);

        Candle previousCandle = null!;
        for (int i = 0; i < candles.Count(); i++)
        {
            if (i == 0)
            {
                previousCandle = candles.ElementAt(i);
                continue;
            }

            Candle candle = candles.ElementAt(i);

            double totalSeconds = Math.Abs((candle.Date - previousCandle.Date).TotalSeconds);

            if (_timeFrame != 0 && (totalSeconds < _timeFrame - 1 || totalSeconds > _timeFrame + 1))
                throw new ArgumentException($"Invalid candle provided.{_candles.Last().Date}");

            _candles = _candles.Append(candle);

            previousCandle = candle;
        }
    }

    public void AddCandle(Candle candle)
    {
        if (_candles.Count() < 2)
        {
            _candles = _candles.Append(candle);
            return;
        }

        if (_timeFrame == 0)
            _timeFrame = Math.Abs((int)(_candles.ElementAt(0).Date - _candles.ElementAt(1).Date).TotalSeconds);

        double totalSeconds = Math.Abs((_candles.Last().Date - candle.Date).TotalSeconds);

        int v = _candles.Count();
        Candle candle1 = _candles.Last();

        if (_timeFrame != 0 && (totalSeconds < _timeFrame - 1 || totalSeconds > _timeFrame + 1))
            throw new ArgumentException($"Invalid candle provided.{_candles.Last().Date}");

        _candles = _candles.Append(candle);
        return;
    }

    public void Skip(int num) => _candles = _candles.Skip(num);
    public void SkipLast(int num) => _candles = _candles.SkipLast(num);
    public void Take(int num) => _candles = _candles.Take(num);
    public void TakeLast(int num) => _candles = _candles.TakeLast(num);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<Candle> GetEnumerator()
    {
        foreach (Candle candle in _candles)
            yield return candle;
    }
}
