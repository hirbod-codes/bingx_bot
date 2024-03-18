using bot.src.Data.Models;
using bot.src.MessageStores;
using bot.src.MessageStores.InMemory.Models;
using bot.src.Notifiers;
using Serilog;
using Skender.Stock.Indicators;
using bot.src.Indicators;
using bot.src.Indicators.CandlesOpenClose;
using bot.src.Strategies.CandlesOpenClose.Exceptions;
using bot.src.Brokers;
using bot.src.Data;
using Bogus;

namespace bot.src.Strategies.CandlesOpenClose;

public class Strategy : IStrategy
{
    private readonly IndicatorOptions _indicatorsOptions;
    private readonly StrategyOptions _strategyOptions;
    private IEnumerable<StochResult> _stochastic = null!;
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

        _stochastic = candles.GetStoch(_indicatorsOptions.Stochastic.Period, _indicatorsOptions.Stochastic.SignalPeriod, _indicatorsOptions.Stochastic.SmoothPeriod);
        _rsi = candles.GetRsi();
    }

    public async Task HandleCandle(Candle candle, int timeFrame)
    {
        if (_stochastic == null)
            throw new NoIndicatorException();

        int index = await _broker.GetLastCandleIndex();

        bool isLong = IsLong();

        DateTime candleCloseDate = candle.Date.AddSeconds(timeFrame);

        IEnumerable<Position> pendingPositions = (await _broker.GetPendingPositions()).Where(o => o != null)!;

        if (pendingPositions.Count() == 2)
            return;

        if (pendingPositions.Count() == 1)
            await _broker.CancelAllPendingPositions();

        bool shouldOpenPosition = false;
        if (!pendingPositions.Any())
            if (!_strategyOptions.InvalidWeekDays.Any() || (_strategyOptions.InvalidWeekDays.Any() && !_strategyOptions.InvalidWeekDays.Where(invalidDate => candleCloseDate.DayOfWeek == invalidDate).Any()))
                if (!_strategyOptions.InvalidTimePeriods.Any() || (_strategyOptions.InvalidTimePeriods.Any() && !_strategyOptions.InvalidTimePeriods.Where(invalidTimePeriod => candleCloseDate.TimeOfDay <= invalidTimePeriod.End.TimeOfDay && candleCloseDate.TimeOfDay >= invalidTimePeriod.Start.TimeOfDay).Any()))
                    if (!_strategyOptions.InvalidDatePeriods.Any() || (_strategyOptions.InvalidDatePeriods.Any() && !_strategyOptions.InvalidDatePeriods.Where(invalidDatePeriod => candleCloseDate.Date <= invalidDatePeriod.End.Date && candleCloseDate.Date >= invalidDatePeriod.Start.Date).Any()))
                        shouldOpenPosition = true;

        if (shouldOpenPosition)
        {
            _logger.Information("Candle is valid for a position, sending the message...");

            decimal delta = CalculateDelta(candle);

            if (delta < 10)
            {
                _logger.Information("Candle's body is too small for a position, Skipping...");
                return;
            }

            decimal offset = 50;

            decimal topBody = Math.Max(candle.Close, candle.Open) + offset;
            decimal bottomBody = Math.Min(candle.Close, candle.Open) - offset;

            decimal longTpPrice = topBody + (delta * _strategyOptions.RiskRewardRatio);
            decimal longSlPrice = bottomBody;

            decimal shortTpPrice = bottomBody - (delta * _strategyOptions.RiskRewardRatio);
            decimal shortSlPrice = topBody;

            IMessage longMessage = CreateOpenPositionMessage(candle, timeFrame, PositionDirection.LONG, topBody, longSlPrice, longTpPrice);
            IMessage shortMessage = CreateOpenPositionMessage(candle, timeFrame, PositionDirection.SHORT, bottomBody, shortSlPrice, shortTpPrice);

            await _messageRepository.CreateMessage(longMessage);
            await _messageRepository.CreateMessage(shortMessage);
            _logger.Information("Messages sent.");
        }
    }

    private decimal CalculateDelta(Candle candle) => Math.Abs(candle.Close - candle.Open);

    private bool IsLong() => new Faker().Random.Bool();

    private IMessage CreateOpenPositionMessage(Candle candle, int timeFrame, string direction, decimal entryPrice, decimal slPrice, decimal tpPrice) => new Message()
    {
        From = _strategyOptions.ProviderName,
        Body = Bots.CandlesOpenClose.Models.Message.CreateMessageBody(
            openingPosition: true,
            allowingParallelPositions: false,
            closingAllPositions: false,
            direction,
            entryPrice,
            slPrice,
            tpPrice
        ),
        SentAt = candle.Date.AddSeconds(timeFrame)
    };
}