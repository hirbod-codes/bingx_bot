using bot.src.Data.Models;
using bot.src.MessageStores;
using bot.src.MessageStores.InMemory.Models;
using bot.src.Notifiers;
using Serilog;
using Skender.Stock.Indicators;
using bot.src.Indicators;
using bot.src.Indicators.EmaStochasticSuperTrend;
using bot.src.Strategies.EmaStochasticSuperTrend.Exceptions;
using bot.src.Bots.General.Models;
using bot.src.Brokers;
using bot.src.Data;
using Bogus;
using bot.src.RiskManagement;

namespace bot.src.Strategies.EmaStochasticSuperTrend;

public class Strategy : IStrategy
{
    private readonly IndicatorOptions _indicatorsOptions;
    private readonly StrategyOptions _strategyOptions;
    private IEnumerable<AtrResult> _atr = null!;
    private IEnumerable<EmaResult> _ema1 = null!;
    private IEnumerable<EmaResult> _ema2 = null!;
    private IEnumerable<StochResult> _stochastic = null!;
    private IEnumerable<SuperTrendResult> _superTrend = null!;
    private readonly IRiskManagement _riskManagement;
    private readonly IBroker _broker;
    private readonly INotifier _notifier;
    private readonly IMessageRepository _messageRepository;
    private readonly ILogger _logger;

    public Strategy(IStrategyOptions strategyOptions, IIndicatorOptions indicatorsOptions, IRiskManagement riskManagement, IBroker broker, INotifier notifier, IMessageRepository messageRepository, ILogger logger)
    {
        _indicatorsOptions = (indicatorsOptions as IndicatorOptions)!;
        _strategyOptions = (strategyOptions as StrategyOptions)!;
        _riskManagement = riskManagement;
        _broker = broker;
        _notifier = notifier;
        _messageRepository = messageRepository;
        _logger = logger.ForContext<Strategy>();
    }

    public async Task PrepareIndicators()
    {
        Candles candles = await _broker.GetCandles();

        _atr = candles.GetAtr(_indicatorsOptions.Atr.Period);
        _ema1 = candles.GetEma(_indicatorsOptions.Ema1.Period);
        _ema2 = candles.GetEma(_indicatorsOptions.Ema2.Period);
        _stochastic = candles.GetStoch(_indicatorsOptions.Stochastic.Period, _indicatorsOptions.Stochastic.SignalPeriod, _indicatorsOptions.Stochastic.SmoothPeriod);
        _superTrend = candles.GetSuperTrend(_indicatorsOptions.SuperTrend.Period, _indicatorsOptions.SuperTrend.Multiplier);
    }

    public Dictionary<string, object> GetIndicators() => new(new KeyValuePair<string, object>[]{
            new(nameof(_atr), _atr),
            new(nameof(_ema1), _ema1),
            new(nameof(_ema2), _ema2),
            new(nameof(_stochastic), _stochastic),
            new(nameof(_superTrend), _superTrend)
        });

    public async Task HandleCandle(Candle candle, int timeFrame)
    {
        if (_atr == null)
            throw new NoIndicatorException();

        int index = await _broker.GetLastCandleIndex();

        DateTime candleCloseDate = candle.Date.AddSeconds(timeFrame);

        bool isLong = _stochastic.ElementAt(index).K >= 80 && _ema2.ElementAt(index).Ema < _ema1.ElementAt(index).Ema;
        bool isShort = _stochastic.ElementAt(index).K <= 20 && _ema2.ElementAt(index).Ema > _ema1.ElementAt(index).Ema;

        bool shouldOpenPosition = false;
        if (isLong)
            // if (isLong || isShort)
            if (!_strategyOptions.InvalidWeekDays.Any() || (_strategyOptions.InvalidWeekDays.Any() && !_strategyOptions.InvalidWeekDays.Where(invalidDate => candleCloseDate.DayOfWeek == invalidDate).Any()))
                if (!_strategyOptions.InvalidTimePeriods.Any() || (_strategyOptions.InvalidTimePeriods.Any() && !_strategyOptions.InvalidTimePeriods.Where(invalidTimePeriod => candleCloseDate.TimeOfDay <= invalidTimePeriod.End.TimeOfDay && candleCloseDate.TimeOfDay >= invalidTimePeriod.Start.TimeOfDay).Any()))
                    if (!_strategyOptions.InvalidDatePeriods.Any() || (_strategyOptions.InvalidDatePeriods.Any() && !_strategyOptions.InvalidDatePeriods.Where(invalidDatePeriod => candleCloseDate.Date <= invalidDatePeriod.End.Date && candleCloseDate.Date >= invalidDatePeriod.Start.Date).Any()))
                        shouldOpenPosition = true;

        if (shouldOpenPosition && isLong)
        {
            _logger.Information("Candle is valid for a position, sending the message...");

            decimal delta = CalculateDelta(index);

            decimal slPrice = CalculateSlPrice(candle.Close, true, delta);
            decimal tpPrice = CalculateTpPrice(candle.Close, true, delta);

            IMessage message = CreateOpenPositionMessage(candle, timeFrame, PositionDirection.LONG, slPrice, tpPrice);
            await _messageRepository.CreateMessage(message);

            _logger.Information("Message sent.");
        }

        if (shouldOpenPosition && isShort)
        {
            _logger.Information("Candle is valid for a position, sending the message...");

            decimal delta = CalculateDelta(index);

            decimal slPrice = CalculateSlPrice(candle.Close, false, delta);
            decimal tpPrice = CalculateTpPrice(candle.Close, false, delta);

            IMessage message = CreateOpenPositionMessage(candle, timeFrame, PositionDirection.SHORT, slPrice, tpPrice);
            await _messageRepository.CreateMessage(message);

            _logger.Information("Message sent.");
        }
    }

    // private bool IsRibbonUp(int index) => _wma7.ElementAt(index).Wma <= _wma6.ElementAt(index).Wma && _wma6.ElementAt(index).Wma <= _wma5.ElementAt(index).Wma && _wma5.ElementAt(index).Wma <= _wma4.ElementAt(index).Wma && _wma4.ElementAt(index).Wma <= _wma3.ElementAt(index).Wma && _wma3.ElementAt(index).Wma <= _wma2.ElementAt(index).Wma && _wma2.ElementAt(index).Wma <= _wma1.ElementAt(index).Wma;

    // private bool IsRibbonDown(int index) => _wma7.ElementAt(index).Wma >= _wma6.ElementAt(index).Wma && _wma6.ElementAt(index).Wma >= _wma5.ElementAt(index).Wma && _wma5.ElementAt(index).Wma >= _wma4.ElementAt(index).Wma && _wma4.ElementAt(index).Wma >= _wma3.ElementAt(index).Wma && _wma3.ElementAt(index).Wma >= _wma2.ElementAt(index).Wma && _wma2.ElementAt(index).Wma >= _wma1.ElementAt(index).Wma;

    private decimal CalculateDelta(int index) => (decimal)(_atr.ElementAt(index).Atr * _indicatorsOptions.AtrMultiplier)!;

    private decimal CalculateSlPrice(decimal entryPrice, bool isUpTrend, decimal delta) => isUpTrend ? entryPrice - delta : entryPrice + delta;

    private decimal CalculateTpPrice(decimal entryPrice, bool isUpTrend, decimal delta) => isUpTrend ? entryPrice + (delta * _strategyOptions.RiskRewardRatio) : entryPrice - (delta * _strategyOptions.RiskRewardRatio);

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
}
