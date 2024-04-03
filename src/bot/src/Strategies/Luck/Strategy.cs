using bot.src.Data.Models;
using bot.src.MessageStores;
using bot.src.MessageStores.InMemory.Models;
using bot.src.Notifiers;
using Serilog;
using Skender.Stock.Indicators;
using bot.src.Indicators;
using bot.src.Indicators.Luck;
using bot.src.Strategies.Luck.Exceptions;
using bot.src.Bots.General.Models;
using bot.src.Brokers;
using bot.src.Data;
using Bogus;

namespace bot.src.Strategies.Luck;

public class Strategy : IStrategy
{
    private readonly IndicatorOptions _indicatorsOptions;
    private readonly StrategyOptions _strategyOptions;
    private IEnumerable<AtrResult> _atr = null!;
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
        Candles candles = await _broker.GetCandles() ?? throw new CandlesNotFoundException();

        _atr = candles.GetAtr(_indicatorsOptions.Atr.Period);
        _rsi = candles.GetRsi();
    }

    public Dictionary<string, object> GetIndicators() => new(new KeyValuePair<string, object>[]{
            new(nameof(_atr), _atr),
            new(nameof(_rsi), _rsi)
        });

    public async Task HandleCandle(Candle candle, int timeFrame)
    {
        if (_atr == null)
            throw new NoIndicatorException();

        int index = (int)await _broker.GetLastCandleIndex();

        bool isLong = IsLong();

        DateTime candleCloseDate = candle.Date.AddSeconds(timeFrame);

        bool shouldOpenPosition = false;
        if ((isLong && _rsi.ElementAt(index).Rsi < 70) || (!isLong && _rsi.ElementAt(index).Rsi > 30))
            if (!_strategyOptions.InvalidWeekDays.Any() || (_strategyOptions.InvalidWeekDays.Any() && !_strategyOptions.InvalidWeekDays.Where(invalidDate => candleCloseDate.DayOfWeek == invalidDate).Any()))
                if (!_strategyOptions.InvalidTimePeriods.Any() || (_strategyOptions.InvalidTimePeriods.Any() && !_strategyOptions.InvalidTimePeriods.Where(invalidTimePeriod => candleCloseDate.TimeOfDay <= invalidTimePeriod.End.TimeOfDay && candleCloseDate.TimeOfDay >= invalidTimePeriod.Start.TimeOfDay).Any()))
                    if (!_strategyOptions.InvalidDatePeriods.Any() || (_strategyOptions.InvalidDatePeriods.Any() && !_strategyOptions.InvalidDatePeriods.Where(invalidDatePeriod => candleCloseDate.Date <= invalidDatePeriod.End.Date && candleCloseDate.Date >= invalidDatePeriod.Start.Date).Any()))
                        shouldOpenPosition = true;

        if (shouldOpenPosition)
        {
            _logger.Information("Candle is valid for a position, sending the message...");
            _logger.Information("Position direction: {direction}", isLong ? PositionDirection.LONG : PositionDirection.SHORT);

            decimal delta = CalculateDelta(index);

            decimal slPrice = CalculateSlPrice(candle.Close, isLong, delta);
            decimal tpPrice = CalculateTpPrice(candle.Close, isLong, delta);

            IMessage message = CreateOpenPositionMessage(candle, timeFrame, isLong ? PositionDirection.LONG : PositionDirection.SHORT, slPrice, tpPrice);
            await _messageRepository.CreateMessage(message);
            _logger.Information("Message sent.");
        }
    }

    private decimal CalculateDelta(int index) => (decimal)(_atr.ElementAt(index).Atr * _indicatorsOptions.AtrMultiplier)!;

    private decimal CalculateSlPrice(decimal entryPrice, bool isUpTrend, decimal delta) => isUpTrend ? entryPrice - delta : entryPrice + delta;

    private decimal CalculateTpPrice(decimal entryPrice, bool isUpTrend, decimal delta) => isUpTrend ? entryPrice + (delta * _strategyOptions.RiskRewardRatio) : entryPrice - (delta * _strategyOptions.RiskRewardRatio);

    private bool IsLong() => new Faker().Random.Bool();

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
