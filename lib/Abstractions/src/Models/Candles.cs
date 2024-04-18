using System.Collections;
using System.Collections.ObjectModel;

namespace Abstractions.src.Models;

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

    private Collection<Candle> _candles;

    public Candles(IEnumerable<Candle> candles)
    {
        _candles = new Collection<Candle>(candles.ToList());
    }

    public Candles(List<Candle> candles)
    {
        Prepare(ref candles);
        _candles = new Collection<Candle>(candles);
    }

    public void Add(Candle candle)
    {
        if (_candles.Count < 2)
        {
            _candles.Add(candle);
            return;
        }

        if ((_candles.Last().Date - candle.Date).TotalSeconds == 0)

            if (_timeFrame == 0)
                _timeFrame = Math.Abs((int)(_candles.ElementAt(0).Date - _candles.ElementAt(1).Date).TotalSeconds);

        double totalSeconds = Math.Abs((_candles.Last().Date - candle.Date).TotalSeconds);

        if (totalSeconds < 1 && totalSeconds > 0)
            return;

        if (_timeFrame != 0 && (totalSeconds < _timeFrame - 1 || totalSeconds > _timeFrame + 1))
            throw new ArgumentException($"Invalid candle provided.{_candles.Last().Date}");

        _candles.Add(candle);
        return;
    }

    public void AppendRange(IEnumerable<Candle> candles)
    {
        if (!candles.Any())
            return;

        List<Candle> candlesList = candles.ToList();

        Prepare(ref candlesList);

        if (!_candles.Any())
            _candles = new Collection<Candle>(candlesList.ToList());
        else
            _candles = new Collection<Candle>(_candles.Concat(candlesList).ToList());
    }
    public void AppendRange(Candles candles)
    {
        if (!candles.Any())
            return;

        Prepare(candles);

        if (!_candles.Any())
            _candles = new Collection<Candle>(candles.ToList());
        else
            _candles = new Collection<Candle>(_candles.Concat(candles).ToList());
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

    public Candles Skip(int num)
    {
        _candles = new(_candles.Skip(num).ToList());
        return this;
    }

    public Candles SkipLast(int num)
    {
        _candles = new(_candles.SkipLast(num).ToList());
        return this;
    }

    public Candles SkipWhile(Func<Candle, bool> predicate)
    {
        _candles = new(_candles.SkipWhile(predicate).ToList());
        return this;
    }

    public Candles Take(int num)
    {
        _candles = new(_candles.Take(num).ToList());
        return this;
    }

    public Candles TakeLast(int num)
    {
        _candles = new(_candles.TakeLast(num).ToList());
        return this;
    }

    public Candles TakeWhile(Func<Candle, bool> predicate)
    {
        _candles = new(_candles.TakeWhile(predicate).ToList());
        return this;
    }

    public IEnumerator<Candle> GetEnumerator() => _candles.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private void Prepare(ref List<Candle> candles)
    {
        if (candles.Count < 2)
            return;

        if (_timeFrame == 0)
            _timeFrame = Math.Abs((int)(candles.ElementAt(0).Date - candles.ElementAt(1).Date).TotalSeconds);

        List<Candle> candlesTemp = new() { candles[0] };

        int count = candles.Count;
        for (int i = 1; i < count; i++)
        {
            Candle previousCandle = candles.ElementAt(i - 1);
            Candle candle = candles.ElementAt(i);

            double totalSeconds = Math.Abs((candle.Date - previousCandle.Date).TotalSeconds);

            if (totalSeconds == 0)
                continue;

            if (totalSeconds < _timeFrame - 1 || totalSeconds > _timeFrame + 1)
                throw new ArgumentException($"Invalid candle provided.{candle.Date}");

            candlesTemp.Add(candle);
        }

        candles = candlesTemp;
    }

    private void Prepare(Candles candles)
    {
        if (!candles.Any())
            return;

        double totalSeconds = (_candles.Last().Date - candles.First().Date).TotalSeconds;

        if (totalSeconds < _timeFrame - 1 || totalSeconds > _timeFrame + 1)
            throw new ArgumentException($"Invalid candle provided: {candles.First().Date}");
    }

    public void Unshift(List<Candle> candles)
    {
        Prepare(ref candles);

        _candles = new(candles.Concat(_candles).ToList());
    }

    public static decimal High(IEnumerable<Candle> candles)
    {
        decimal highestHigh = 0;
        for (int i = 0; i < candles.Count(); i++)
            if (candles.ElementAt(i).High > highestHigh)
                highestHigh = candles.ElementAt(i).High;
        return highestHigh;
    }

    public static decimal Low(IEnumerable<Candle> candles)
    {
        decimal lowestLow = decimal.MaxValue;
        for (int i = 0; i < candles.Count(); i++)
            if (candles.ElementAt(i).Low < lowestLow)
                lowestLow = candles.ElementAt(i).High;
        return lowestLow;
    }
}
