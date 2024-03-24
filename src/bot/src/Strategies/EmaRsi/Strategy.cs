using bot.src.Data.Models;
using bot.src.MessageStores;
using bot.src.MessageStores.InMemory.Models;
using bot.src.Notifiers;
using Serilog;
using Skender.Stock.Indicators;
using bot.src.Indicators;
using bot.src.Indicators.EmaRsi;
using bot.src.Strategies.EmaRsi.Exceptions;
using bot.src.Bots.General.Models;
using bot.src.Brokers;
using bot.src.Data;

namespace bot.src.Strategies.EmaRsi;

public class Strategy : IStrategy
{
    private readonly IndicatorOptions _indicatorsOptions;
    private readonly StrategyOptions _strategyOptions;
    private IEnumerable<SuperTrendResult> _superTrend = null!;
    private IEnumerable<AtrResult> _atr = null!;
    private IEnumerable<EmaResult> _ema1 = null!;
    private IEnumerable<EmaResult> _ema2 = null!;
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
        if (_indicatorsOptions.Ema1.Period == _indicatorsOptions.Ema2.Period)
            throw new NoIndicatorException("Missing second ema indicator options.");

        Candles candles = await _broker.GetCandles();

        _superTrend = candles.GetSuperTrend(20);
        _atr = candles.GetAtr(_indicatorsOptions.Atr.Period);
        _ema1 = candles.GetEma(_indicatorsOptions.Ema1.Period);
        _ema2 = candles.GetEma(_indicatorsOptions.Ema2.Period);
        _rsi = candles.GetRsi(_indicatorsOptions.Rsi.Period);
    }

    public Dictionary<string, object> GetIndicators() => new(new KeyValuePair<string, object>[]{
            new(nameof(_superTrend), _superTrend),
            new(nameof(_atr), _atr),
            new(nameof(_ema1), _ema1),
            new(nameof(_ema2), _ema2),
            new(nameof(_rsi), _rsi)
        });

    public async Task HandleCandle(Candle candle, int timeFrame)
    {
        if (_atr == null || _ema1 == null || _ema2 == null || _rsi == null)
            throw new NoIndicatorException();

        int index = await _broker.GetLastCandleIndex();

        bool isUpTrend = IsUpTrend(index);
        if (!isUpTrend)
            _logger.Information("isUpTrend: {isUpTrend}", isUpTrend);

        bool rsiCrossedOverLowerBand = HasRsiCrossedOverLowerBand(index);
        bool rsiCrossedUnderUpperBand = HasRsiCrossedUnderUpperBand(index);

        DateTime candleCloseDate = candle.Date.AddSeconds(timeFrame);

        bool shouldOpenPosition = false;
        if ((isUpTrend && rsiCrossedOverLowerBand) || (!isUpTrend && rsiCrossedUnderUpperBand))
            if (!_strategyOptions.InvalidWeekDays.Any() || (_strategyOptions.InvalidWeekDays.Any() && !_strategyOptions.InvalidWeekDays.Where(invalidDate => candleCloseDate.DayOfWeek == invalidDate).Any()))
                if (!_strategyOptions.InvalidTimePeriods.Any() || (_strategyOptions.InvalidTimePeriods.Any() && !_strategyOptions.InvalidTimePeriods.Where(invalidTimePeriod => candleCloseDate.TimeOfDay <= invalidTimePeriod.End.TimeOfDay && candleCloseDate.TimeOfDay >= invalidTimePeriod.Start.TimeOfDay).Any()))
                    if (!_strategyOptions.InvalidDatePeriods.Any() || (_strategyOptions.InvalidDatePeriods.Any() && !_strategyOptions.InvalidDatePeriods.Where(invalidDatePeriod => candleCloseDate.Date <= invalidDatePeriod.End.Date && candleCloseDate.Date >= invalidDatePeriod.Start.Date).Any()))
                        shouldOpenPosition = true;

        if (shouldOpenPosition)
        {
            _logger.Information("Candle is valid for a position, sending the message...");
            _logger.Information("Position direction: {direction}", isUpTrend ? PositionDirection.LONG : PositionDirection.SHORT);

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

    private bool IsUpTrend(int index) => _ema1.ElementAt(index).Ema >= _ema2.ElementAt(index).Ema;

    private bool HasRsiCrossedUnderUpperBand(int index) => _rsi.ElementAt(index - 1).Rsi > _indicatorsOptions.Rsi.UpperBand && _rsi.ElementAt(index).Rsi <= _indicatorsOptions.Rsi.UpperBand;

    private bool HasRsiCrossedOverLowerBand(int index) => _rsi.ElementAt(index - 1).Rsi < _indicatorsOptions.Rsi.LowerBand && _rsi.ElementAt(index).Rsi >= _indicatorsOptions.Rsi.LowerBand;

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
