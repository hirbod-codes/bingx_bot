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
using bot.src.Data;

namespace bot.src.Strategies.SmmaRsi;

public class Strategy : IStrategy
{
    private readonly IndicatorOptions _indicatorsOptions;
    private readonly StrategyOptions _strategyOptions;
    private IEnumerable<AtrResult> _atr = null!;
    private IEnumerable<SmmaResult> _smma1 = null!;
    private IEnumerable<SmmaResult> _smma2 = null!;
    private IEnumerable<SmmaResult> _smma3 = null!;
    private IEnumerable<RsiResult> _rsi = null!;
    private readonly IBroker _broker;
    private readonly INotifier _notifier;
    private readonly IMessageRepository _messageRepository;
    private readonly ILogger _logger;

    public Strategy(IStrategyOptions strategyOptions, IIndicatorOptions indicatorsOptions, IBroker broker, INotifier notifier, IMessageRepository messageRepository, ILogger logger)
    {
        _indicatorsOptions = (indicatorsOptions as IndicatorOptions)!;
        _strategyOptions = (strategyOptions as StrategyOptions)!;
        _broker = broker;
        _notifier = notifier;
        _messageRepository = messageRepository;
        _logger = logger.ForContext<Strategy>();
    }

    public async Task PrepareIndicators()
    {
        Candles candles = await _broker.GetCandles();

        _atr = candles.GetAtr(_indicatorsOptions.Atr.Period);
        _smma1 = candles.GetSmma(_indicatorsOptions.Smma1.Period);
        _smma2 = candles.GetSmma(_indicatorsOptions.Smma2.Period);
        _smma3 = candles.GetSmma(_indicatorsOptions.Smma3.Period);
        _rsi = candles.GetRsi(_indicatorsOptions.Rsi.Period);
    }

    public Dictionary<string, object> GetIndicators() => new(new KeyValuePair<string, object>[]{
            new(nameof(_atr), _atr),
            new(nameof(_smma1), _smma1),
            new(nameof(_smma2), _smma2),
            new(nameof(_smma3), _smma3),
            new(nameof(_rsi), _rsi)
        });


    public async Task HandleCandle(Candle candle, int timeFrame)
    {
        if ((candle.High - candle.Low) > 70)
        {
            _logger.Information("This candle is too big, skipping...");
            return;
        }

        if (_smma1 == null || _smma2 == null || _smma3 == null || _rsi == null)
            throw new NoIndicatorException();

        int index = await _broker.GetLastCandleIndex();

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
                            shouldOpenPosition = true;

        if (shouldOpenPosition)
        {
            _logger.Information("Candle is valid for a position, sending the message...");

            decimal delta = CalculateDelta(index);

            decimal slPrice = CalculateSlPrice(candle.Close, isUpTrend, delta);
            decimal tpPrice = CalculateTpPrice(candle.Close, isUpTrend, delta);

            IMessage message = CreateOpenPositionMessage(candle, timeFrame, isUpTrend ? PositionDirection.LONG : PositionDirection.SHORT, slPrice, tpPrice);
            await _messageRepository.CreateMessage(message);
            _logger.Information("Message sent.");
        }
    }

    private decimal CalculateDelta(int index) => (decimal)(_atr.ElementAt(index).Atr * _indicatorsOptions.AtrMultiplier)!;

    private decimal CalculateSlPrice(decimal entryPrice, bool isUpTrend, decimal delta) => isUpTrend ? entryPrice - delta : entryPrice + delta;

    private decimal CalculateTpPrice(decimal entryPrice, bool isUpTrend, decimal delta) => isUpTrend ? entryPrice + (delta * _strategyOptions.RiskRewardRatio) : entryPrice - (delta * _strategyOptions.RiskRewardRatio);

    private bool HasRsiCrossedUnderUpperBand(int index)
    {
        if (_rsi.ElementAt(index).Rsi is null || _rsi.ElementAt(index - 1).Rsi is null)
            return false;

        return _rsi.ElementAt(index - 1).Rsi > _indicatorsOptions.Rsi.UpperBand && _rsi.ElementAt(index).Rsi < _indicatorsOptions.Rsi.UpperBand;
    }

    private bool HasRsiCrossedOverLowerBand(int index)
    {
        if (_rsi.ElementAt(index).Rsi is null || _rsi.ElementAt(index - 1).Rsi is null)
            return false;

        return _rsi.ElementAt(index - 1).Rsi < _indicatorsOptions.Rsi.LowerBand && _rsi.ElementAt(index).Rsi > _indicatorsOptions.Rsi.LowerBand;
    }

    private bool IsDownTrend(int index)
    {
        if (_smma1.ElementAt(index).Smma is null || _smma2.ElementAt(index).Smma is null || _smma3.ElementAt(index).Smma is null)
            return false;

        return _smma1.ElementAt(index).Smma <= _smma2.ElementAt(index).Smma && _smma2.ElementAt(index).Smma <= _smma3.ElementAt(index).Smma;
    }

    private bool IsUpTrend(int index)
    {
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
