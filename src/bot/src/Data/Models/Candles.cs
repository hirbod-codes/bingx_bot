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

    private List<Candle> _candles;

    public Candles(IEnumerable<Candle> candles)
    {
        Validate(candles.ToList());
        _candles = candles.ToList();
    }

    public Candles(List<Candle> candles)
    {
        Validate(candles);
        _candles = candles;
    }

    private void Validate(List<Candle> candles)
    {
        _candles = new();

        if (!candles.Any())
            throw new ArgumentException("No candle provided.");

        if (candles.Count == 1)
        {
            _candles = candles.ToList();
            return;
        }

        if (_timeFrame == 0)
            _timeFrame = Math.Abs((int)(candles.ElementAt(0).Date - candles.ElementAt(1).Date).TotalSeconds);

        Candle previousCandle = null!;
        int count = candles.Count;
        for (int i = 0; i < count; i++)
        {
            Candle candle = candles.ElementAt(i);

            if (i == 0)
            {
                previousCandle = candles.ElementAt(i);
                _candles.Add(candle);
                continue;
            }

            double totalSeconds = Math.Abs((candle.Date - previousCandle.Date).TotalSeconds);

            if (_timeFrame != 0 && (totalSeconds < _timeFrame - 1 || totalSeconds > _timeFrame + 1))
                throw new ArgumentException($"Invalid candle provided.{_candles.Last().Date}");

            _candles.Add(candle);

            previousCandle = candle;
        }
    }

    public void Add(Candle candle)
    {
        if (_candles.Count() < 2)
        {
            _candles.Add(candle);
            return;
        }

        if (_timeFrame == 0)
            _timeFrame = Math.Abs((int)(_candles.ElementAt(0).Date - _candles.ElementAt(1).Date).TotalSeconds);

        double totalSeconds = Math.Abs((_candles.Last().Date - candle.Date).TotalSeconds);

        int v = _candles.Count();
        Candle candle1 = _candles.Last();

        if (_timeFrame != 0 && (totalSeconds < _timeFrame - 1 || totalSeconds > _timeFrame + 1))
            throw new ArgumentException($"Invalid candle provided.{_candles.Last().Date}");

        _candles.Add(candle);
        return;
    }

    public IEnumerable<Candle> Prepend(IEnumerable<Candle> candles)
    {
        foreach (var x in candles)
            yield return x;

        foreach (var x in _candles)
            yield return x;
    }

    public IEnumerable<Candle> Prepend(Candle candle)
    {
        yield return candle;

        foreach (var x in _candles)
            yield return x;
    }

    public void Skip(int num) => _candles = _candles.Skip(num).ToList();
    public void SkipLast(int num) => _candles = _candles.SkipLast(num).ToList();
    public void Take(int num) => _candles = _candles.Take(num).ToList();
    public void TakeLast(int num) => _candles = _candles.TakeLast(num).ToList();

    public IEnumerator<Candle> GetEnumerator() => _candles.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
