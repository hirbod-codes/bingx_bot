using StrategySimulator.src.Data;
using StrategySimulator.src.Indicators;
using StrategySimulator.src.Models;

namespace StrategySimulator.src;

public class SmmaRsiStrategySimulator : IStrategySimulator
{
    private readonly Candles _candles;
    private readonly IIndicator _smma1;
    private readonly IIndicator _smma2;
    private readonly IIndicator _smma3;
    private readonly IIndicator _rsi;
    private readonly SmmaRsiStrategySimulatorOptions _options;

    public SmmaRsiStrategySimulator(Candles candles, IIndicator smma1, IIndicator smma2, IIndicator smma3, IIndicator rsi, SmmaRsiStrategySimulatorOptions options)
    {
        _candles = candles;
        _smma1 = smma1;
        _smma2 = smma2;
        _smma3 = smma3;
        _rsi = rsi;
        _options = options;
    }

    public Task Simulate()
    {
        for (int i = 0; i < _candles.Count(); i++)
        {
            bool isUpTrend = IsUpTrend(i);
            bool isDownTrend = IsDownTrend(i);
            bool isInTrend = isUpTrend || isDownTrend;

            bool rsiCrossedOverLowerBand = HasRsiCrossedOverLowerBand(i);
            bool rsiCrossedUnderUpperBand = HasRsiCrossedUnderUpperBand(i);

            bool shouldOpenPosition = false;
            if (isInTrend)
                if ((isUpTrend && rsiCrossedOverLowerBand) || (isDownTrend && rsiCrossedUnderUpperBand))
                    shouldOpenPosition = true;
            // 
        }
    }

    private bool HasRsiCrossedUnderUpperBand(int index)
    {
        throw new NotImplementedException();
    }

    private bool HasRsiCrossedOverLowerBand(int index)
    {
        throw new NotImplementedException();
    }

    private bool IsDownTrend(int index)
    {
        throw new NotImplementedException();
    }

    private bool IsUpTrend(int index)
    {
        throw new NotImplementedException();
    }
}

public class SmmaRsiStrategySimulatorOptions
{
}
