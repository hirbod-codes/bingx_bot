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
    private IEnumerable<StochResult> _stochastic = null!;
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

        _superTrend = candles.GetSuperTrend(20, 2);
        _stochastic = candles.GetStoch();
        _atr = candles.GetAtr(_indicatorsOptions.Atr.Period);
        _ema1 = candles.GetEma(_indicatorsOptions.Ema1.Period);
        _ema2 = candles.GetEma(_indicatorsOptions.Ema2.Period);
        _rsi = candles.GetRsi(_indicatorsOptions.Rsi.Period);
    }

    public Dictionary<string, object> GetIndicators() => new(new KeyValuePair<string, object>[]{
            new(nameof(_superTrend), _superTrend),
            new(nameof(_stochastic), _stochastic),
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
        bool isDownTrend = IsDownTrend(index);

        DateTime candleCloseDate = candle.Date.AddSeconds(timeFrame);

        bool shouldOpenPosition = false;
        if (isUpTrend || isDownTrend)
            if (!_strategyOptions.InvalidWeekDays.Any() || (_strategyOptions.InvalidWeekDays.Any() && !_strategyOptions.InvalidWeekDays.Where(invalidDate => candleCloseDate.DayOfWeek == invalidDate).Any()))
                if (!_strategyOptions.InvalidTimePeriods.Any() || (_strategyOptions.InvalidTimePeriods.Any() && !_strategyOptions.InvalidTimePeriods.Where(invalidTimePeriod => candleCloseDate.TimeOfDay <= invalidTimePeriod.End.TimeOfDay && candleCloseDate.TimeOfDay >= invalidTimePeriod.Start.TimeOfDay).Any()))
                    if (!_strategyOptions.InvalidDatePeriods.Any() || (_strategyOptions.InvalidDatePeriods.Any() && !_strategyOptions.InvalidDatePeriods.Where(invalidDatePeriod => candleCloseDate.Date <= invalidDatePeriod.End.Date && candleCloseDate.Date >= invalidDatePeriod.Start.Date).Any()))
                        shouldOpenPosition = true;

        if (ShouldClosePosition(index))
        {
            _logger.Information("Closing all positions...");

            IMessage closeMessage = CreateClosePositionMessage(candle, timeFrame);
            await _messageRepository.CreateMessage(closeMessage);

            _logger.Information("Message sent.");
            return;
        }

        if (!shouldOpenPosition)
            return;

        _logger.Information("Candle is valid for a position, sending the message...");

        decimal delta = CalculateDelta(index);

        decimal slPrice = CalculateSlPrice(candle.Close, isUpTrend, delta);
        decimal? tpPrice = CalculateTpPrice(candle.Close, isUpTrend, delta);

        // tpPrice = null;

        IMessage message = CreateOpenPositionMessage(candle, timeFrame, isUpTrend ? PositionDirection.LONG : PositionDirection.SHORT, slPrice, tpPrice);
        await _messageRepository.CreateMessage(message);
        _logger.Information("Message sent.");
    }

    // private bool ShouldClosePosition(int index) => 
    //     (IsUpTrend(index) && _superTrend.ElementAt(index).UpperBand != null) || 
    //     (IsDownTrend(index) && _superTrend.ElementAt(index).LowerBand != null);
    private bool ShouldClosePosition(int index) => false;

    private decimal CalculateDelta(int index) => (decimal)(_atr.ElementAt(index).Atr * _indicatorsOptions.AtrMultiplier)!;

    private decimal CalculateSlPrice(decimal entryPrice, bool isUpTrend, decimal delta) => isUpTrend ? entryPrice - delta : entryPrice + delta;

    private decimal CalculateTpPrice(decimal entryPrice, bool isUpTrend, decimal delta) => isUpTrend ? entryPrice + (delta * _strategyOptions.RiskRewardRatio) : entryPrice - (delta * _strategyOptions.RiskRewardRatio);

    private bool IsUpTrend(int index) => HasRsiCrossedOverLowerBand(index) && _stochastic.ElementAt(index).K >= 80 && _ema1.ElementAt(index).Ema >= _ema2.ElementAt(index).Ema;
    // private bool IsUpTrend(int index) => HasRsiCrossedOverLowerBand(index) && _superTrend.ElementAt(index).LowerBand != null;
    // private bool IsUpTrend(int index) => HasRsiCrossedOverLowerBand(index) && _ema1.ElementAt(index).Ema >= _ema2.ElementAt(index).Ema;

    private bool HasRsiCrossedOverLowerBand(int index) => _rsi.ElementAt(index).Rsi >= 30 && _rsi.ElementAt(index).Rsi <= 70;
    // private bool HasRsiCrossedOverLowerBand(int index) => _rsi.ElementAt(index - 1).Rsi < _indicatorsOptions.Rsi.LowerBand && _rsi.ElementAt(index).Rsi >= _indicatorsOptions.Rsi.LowerBand;

    private bool IsDownTrend(int index) => HasRsiCrossedUnderUpperBand(index) && _stochastic.ElementAt(index).K <= 20 && _ema1.ElementAt(index).Ema < _ema2.ElementAt(index).Ema;
    // private bool IsDownTrend(int index) => HasRsiCrossedUnderUpperBand(index) && _superTrend.ElementAt(index).UpperBand != null;
    // private bool IsDownTrend(int index) => HasRsiCrossedUnderUpperBand(index) && _ema1.ElementAt(index).Ema < _ema2.ElementAt(index).Ema;

    private bool HasRsiCrossedUnderUpperBand(int index) => _rsi.ElementAt(index).Rsi >= 30 && _rsi.ElementAt(index).Rsi <= 70;
    // private bool HasRsiCrossedUnderUpperBand(int index) => _rsi.ElementAt(index - 1).Rsi > _indicatorsOptions.Rsi.UpperBand && _rsi.ElementAt(index).Rsi <= _indicatorsOptions.Rsi.UpperBand;

    private IMessage CreateOpenPositionMessage(Candle candle, int timeFrame, string direction, decimal slPrice, decimal? tpPrice) => new Message()
    {
        From = _strategyOptions.ProviderName,
        Body = IGeneralMessage.CreateMessageBody(
                openingPosition: true,
                allowingParallelPositions: false,
                closingAllPositions: false,
                direction,
                slPrice,
                tpPrice
            ),
        SentAt = candle.Date.AddSeconds(timeFrame)
    };

    private IMessage CreateClosePositionMessage(Candle candle, int timeFrame) => new Message()
    {
        From = _strategyOptions.ProviderName,
        Body = IGeneralMessage.CreateMessageBody(
                openingPosition: false,
                allowingParallelPositions: false,
                closingAllPositions: true,
                PositionDirection.LONG,
                0,
                0
            ),
        SentAt = candle.Date.AddSeconds(timeFrame)
    };
}
