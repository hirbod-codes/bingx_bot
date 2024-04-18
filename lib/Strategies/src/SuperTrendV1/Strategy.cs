using ILogger = Serilog.ILogger;
using Skender.Stock.Indicators;
using Abstractions.src.Strategies;
using Abstractions.src.Indicators;
using Abstractions.src.Brokers;
using Abstractions.src.Notifiers;
using Abstractions.src.Repository;
using Indicators.src.SuperTrendV1;
using Abstractions.src.Models;
using Strategies.src.SuperTrendV1.Models;
using Abstractions.src.Indicators.Models;
using Message = Strategies.src.SuperTrendV1.Models.Message;

namespace Strategies.src.SuperTrendV1;

public class Strategy : IStrategy
{
    private readonly IndicatorOptions _indicatorsOptions;
    private readonly StrategyOptions _strategyOptions;
    private IEnumerable<AtrResult> _atr = null!;
    private IEnumerable<SuperTrendV1Result> _superTrend = null!;
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
        Candles candles = await _broker.GetCandles() ?? throw new CandlesNotFoundException();

        _atr = candles.GetAtr(_indicatorsOptions.Atr.Period);

        _superTrend = candles.GetSuperTrendV1(lookBackPeriods: _indicatorsOptions.SuperTrendOptions.Period, candlePart: _indicatorsOptions.SuperTrendOptions.CandlePart, multiplier: (decimal)_indicatorsOptions.SuperTrendOptions.Multiplier, changeATRCalculationMethod: _indicatorsOptions.SuperTrendOptions.ChangeATRCalculationMethod);
    }

    public Dictionary<string, object> GetIndicators() => new(new KeyValuePair<string, object>[]{
            new(nameof(_atr), _atr),
            new(nameof(_superTrend), _superTrend)
        });

    public async Task HandleCandle(Candle candle, int timeFrame)
    {
        int index = (int)await _broker.GetLastCandleIndex();

        _logger.Information("SuperTrend: {SuperTrend}, BuySignal: {BuySignal}, SellSignal: {SellSignal}", _superTrend.ElementAt(index).SuperTrend, _superTrend.ElementAt(index).BuySignal, _superTrend.ElementAt(index).SellSignal);

        bool isLong = IsLong(index);
        bool isShort = IsShort(index);

        DateTime candleCloseDate = candle.Date.AddSeconds(timeFrame);

        bool shouldOpenPosition = false;
        if (isLong || isShort)
            if (!_strategyOptions.InvalidWeekDays.Any() || (_strategyOptions.InvalidWeekDays.Any() && !_strategyOptions.InvalidWeekDays.Where(invalidDate => candleCloseDate.DayOfWeek == invalidDate).Any()))
                if (!_strategyOptions.InvalidTimePeriods.Any() || (_strategyOptions.InvalidTimePeriods.Any() && !_strategyOptions.InvalidTimePeriods.Where(invalidTimePeriod => candleCloseDate.TimeOfDay <= invalidTimePeriod.End.TimeOfDay && candleCloseDate.TimeOfDay >= invalidTimePeriod.Start.TimeOfDay).Any()))
                    if (!_strategyOptions.InvalidDatePeriods.Any() || (_strategyOptions.InvalidDatePeriods.Any() && !_strategyOptions.InvalidDatePeriods.Where(invalidDatePeriod => candleCloseDate.Date <= invalidDatePeriod.End.Date && candleCloseDate.Date >= invalidDatePeriod.Start.Date).Any()))
                        shouldOpenPosition = true;

        if (shouldOpenPosition)
        {
            _logger.Information("Candle is valid for a position, sending the message...");
            _logger.Information("Position direction: {direction}", isLong ? PositionDirection.LONG : PositionDirection.SHORT);

            IMessage message = CreateOpenPositionMessage(candle, timeFrame, isLong ? PositionDirection.LONG : PositionDirection.SHORT, candle.Close);
            _logger.Information("The Message {@message}.", message);

            await _messageRepository.CreateMessage(message);
            _logger.Information("Message sent.");
        }
    }

    private bool IsLong(int index) => _superTrend.ElementAt(index).BuySignal;

    private bool IsShort(int index) => _superTrend.ElementAt(index).SellSignal;

    private IMessage CreateOpenPositionMessage(Candle candle, int timeFrame, string direction, decimal entryPrice) => new Message()
    {
        From = _strategyOptions.ProviderName,
        Body = Message.CreateMessageBody(
                openingPosition: true,
                allowingParallelPositions: false,
                closingAllPositions: false,
                direction,
                entryPrice
            ),
        SentAt = candle.Date.AddSeconds(timeFrame)
    };
}
