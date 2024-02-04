using bot.src.Data.Models;
using bot.src.MessageStores;
using bot.src.MessageStores.InMemory.Models;
using bot.src.Notifiers;
using Serilog;
using Skender.Stock.Indicators;
using bot.src.Indicators;
using bot.src.Indicators.SmmaRsi;
using bot.src.Strategies.SmmaRsi.Exceptions;
using bot.src.Bots.General.Models;
using bot.src.Brokers;

namespace bot.src.Strategies.SmmaRsi;

public class SmmaRsiStrategy : IStrategy
{
    private readonly IndicatorsOptions _indicatorsOptions;
    private readonly StrategyOptions _strategyOptions;
    private IEnumerable<SmmaResult> _smma1 = null!;
    private IEnumerable<SmmaResult> _smma2 = null!;
    private IEnumerable<SmmaResult> _smma3 = null!;
    private IEnumerable<RsiResult> _rsi = null!;
    private readonly IBroker _broker;
    private readonly INotifier _notifier;
    private readonly ILogger _logger;

    public SmmaRsiStrategy(IStrategyOptions strategyOptions, IIndicatorsOptions indicatorsOptions, IBroker broker, INotifier notifier, ILogger logger)
    {
        _indicatorsOptions = (indicatorsOptions as IndicatorsOptions)!;
        _strategyOptions = (strategyOptions as StrategyOptions)!;
        _broker = broker;
        _notifier = notifier;

        _logger = logger.ForContext<SmmaRsiStrategy>();
    }

    public void InitializeIndicators(Candles candles)
    {
        _smma1 = candles.GetSmma(_indicatorsOptions.Smma1.Period);
        _smma2 = candles.GetSmma(_indicatorsOptions.Smma2.Period);
        _smma3 = candles.GetSmma(_indicatorsOptions.Smma3.Period);
        _rsi = candles.GetRsi(_indicatorsOptions.Rsi.Period);
    }

    public async Task HandleCandle(Candle candle, int index, int timeFrame)
    {
        if (_smma1 == null && _smma2 == null && _smma3 == null && _rsi == null)
            throw new NoIndicatorException();

        bool isUpTrend = IsUpTrend(index);
        bool isDownTrend = IsDownTrend(index);
        bool isInTrend = isUpTrend || isDownTrend;

        bool rsiCrossedOverLowerBand = HasRsiCrossedOverLowerBand(index);
        bool rsiCrossedUnderUpperBand = HasRsiCrossedUnderUpperBand(index);

        DateTime candleCloseDate = candle.Date.AddSeconds(timeFrame);

        bool shouldOpenPosition = false;
        if (isInTrend)
            if ((isUpTrend && rsiCrossedOverLowerBand) || (isDownTrend && rsiCrossedUnderUpperBand))
                if (!_strategyOptions.InvalidWeekDays.Any() || (_strategyOptions.InvalidWeekDays.Any() && !_strategyOptions.InvalidWeekDays.Where(invalidDate => candleCloseDate.DayOfWeek == invalidDate).Any()))
                    if (!_strategyOptions.InvalidTimePeriods.Any() || (_strategyOptions.InvalidTimePeriods.Any() && !_strategyOptions.InvalidTimePeriods.Where(invalidTimePeriod => candleCloseDate.TimeOfDay <= invalidTimePeriod.End.TimeOfDay && candleCloseDate.TimeOfDay >= invalidTimePeriod.Start.TimeOfDay).Any()))
                        if (!_strategyOptions.InvalidDatePeriods.Any() || (_strategyOptions.InvalidDatePeriods.Any() && !_strategyOptions.InvalidDatePeriods.Where(invalidDatePeriod => candleCloseDate.Date <= invalidDatePeriod.End.Date && candleCloseDate.Date >= invalidDatePeriod.Start.Date).Any()))
                        {
                            if (_strategyOptions.NaturalTrendIndicatorLength == 0 || _strategyOptions.NaturalTrendIndicatorLimit == 0)
                                shouldOpenPosition = true;
                            else
                            {
                                decimal highestHigh = candle.High;
                                decimal lowestLow = candle.Low;
                                for (int i = 1; i <= _strategyOptions.NaturalTrendIndicatorLength; i++)
                                {
                                    Candle? c = await _broker.GetCandle(index + i);

                                    if (c == null)
                                    {
                                        shouldOpenPosition = true;
                                        break;
                                    }

                                    if (c.High > highestHigh)
                                        highestHigh = c.High;
                                    if (c.Low < lowestLow)
                                        lowestLow = c.Low;
                                }

                                if (!shouldOpenPosition && (highestHigh - lowestLow) > _strategyOptions.NaturalTrendIndicatorLimit)
                                    shouldOpenPosition = true;
                            }
                        }

        if (shouldOpenPosition)
        {
            _logger.Information("Candle is valid for a position, sending the message...");

            IMessage message = CreateOpenPositionMessage(candle, timeFrame, isUpTrend ? PositionDirection.LONG : PositionDirection.SHORT, isUpTrend ? candle.Close - _strategyOptions.SLDifference : candle.Close + _strategyOptions.SLDifference, isUpTrend ? candle.Close + _strategyOptions.TPDifference : candle.Close - _strategyOptions.TPDifference);
            await _notifier.SendMessage(message);
            _logger.Information("Message sent.");
        }
    }

    private bool HasRsiCrossedUnderUpperBand(int index)
    {
        index = _rsi.Count() - 1 - index;

        if (_rsi.ElementAt(index).Rsi is null || _rsi.ElementAt(index - 1).Rsi is null)
            return false;

        return _rsi.ElementAt(index - 1).Rsi > _indicatorsOptions.Rsi.UpperBand && _rsi.ElementAt(index).Rsi < _indicatorsOptions.Rsi.UpperBand;
    }

    private bool HasRsiCrossedOverLowerBand(int index)
    {
        index = _rsi.Count() - 1 - index;

        if (_rsi.ElementAt(index).Rsi is null || _rsi.ElementAt(index - 1).Rsi is null)
            return false;

        return _rsi.ElementAt(index - 1).Rsi < _indicatorsOptions.Rsi.LowerBand && _rsi.ElementAt(index).Rsi > _indicatorsOptions.Rsi.LowerBand;
    }

    private bool IsDownTrend(int index)
    {
        index = _smma1.Count() - 1 - index;

        if (_smma1.ElementAt(index).Smma is null || _smma2.ElementAt(index).Smma is null || _smma3.ElementAt(index).Smma is null)
            return false;

        return _smma1.ElementAt(index).Smma <= _smma2.ElementAt(index).Smma && _smma2.ElementAt(index).Smma <= _smma3.ElementAt(index).Smma;
    }

    private bool IsUpTrend(int index)
    {
        index = _smma1.Count() - 1 - index;

        if (_smma1.ElementAt(index).Smma is null || _smma2.ElementAt(index).Smma is null || _smma3.ElementAt(index).Smma is null)
            return false;

        return _smma1.ElementAt(index).Smma >= _smma2.ElementAt(index).Smma && _smma2.ElementAt(index).Smma >= _smma3.ElementAt(index).Smma;
    }

    private IMessage CreateOpenPositionMessage(Candle candle, int timeFrame, string direction, decimal slPrice, decimal? tpPrice) => new Message()
    {
        From = _strategyOptions.ProviderName,
        Body = IGeneralMessage.CreateMessageBody(
                openingPosition: true,
                allowingParallelPositions: true,
                closingAllPositions: false,
                direction,
                slPrice,
                tpPrice
            ),
        SentAt = candle.Date.AddSeconds(timeFrame)
    };
}
