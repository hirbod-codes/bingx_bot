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

namespace bot.src.Strategies.CandlesOpenClose;

public class Strategy : IStrategy
{
    private readonly IndicatorOptions _indicatorsOptions;
    private readonly StrategyOptions _strategyOptions;
    private IEnumerable<AtrResult> _atr = Array.Empty<AtrResult>();
    private IEnumerable<StochResult> _stochastic = Array.Empty<StochResult>();
    private IEnumerable<SuperTrendResult> _superTrend = Array.Empty<SuperTrendResult>();
    private IEnumerable<WmaResult> _deltaWma = Array.Empty<WmaResult>();
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

    public Dictionary<string, object> GetIndicators() => new(new KeyValuePair<string, object>[]{
            new(nameof(_atr), _atr),
            new(nameof(_stochastic), _stochastic),
            new(nameof(_superTrend), _superTrend),
            new(nameof(_deltaWma), _deltaWma)
        });

    public async Task PrepareIndicators()
    {
        Candles candles = await _broker.GetCandles();

        _stochastic = candles.GetStoch(_indicatorsOptions.Stochastic.Period, _indicatorsOptions.Stochastic.SignalPeriod, _indicatorsOptions.Stochastic.SmoothPeriod);
        _superTrend = candles.GetSuperTrend(10, 2);
        _atr = candles.GetAtr();

        // _deltaWma = candles.ToList().ConvertAll(candle =>
        //   {
        //       decimal offset = Math.Abs(candle.Close - candle.Open) / 4.0m;

        //       decimal upperBand = Math.Max(candle.Close, candle.Open) + offset;
        //       decimal lowerBand = Math.Min(candle.Close, candle.Open) - offset;

        //       decimal delta = Math.Abs(upperBand - lowerBand);

        //       candle.Close = delta;

        //       return candle;
        //   }).GetWma(14);
    }

    public async Task HandleCandle(Candle candle, int timeFrame)
    {
        int index = await _broker.GetLastCandleIndex();

        DateTime candleCloseDate = candle.Date.AddSeconds(timeFrame);

        IEnumerable<Position> openPositions = (await _broker.GetOpenPositions()).Where(o => o != null)!;
        IEnumerable<Position> pendingPositions = (await _broker.GetPendingPositions()).Where(o => o != null)!;
        IEnumerable<Position> longPendingPositions = pendingPositions.Where(o => o.PositionDirection == PositionDirection.LONG)!;
        IEnumerable<Position> shortPendingPositions = pendingPositions.Where(o => o.PositionDirection == PositionDirection.SHORT)!;

        if (longPendingPositions.Any() && longPendingPositions.Last().OpenedAt <= candle.Date.AddSeconds(-4 * timeFrame))
            await _broker.CancelAllLongPendingPositions();

        if (shortPendingPositions.Any() && shortPendingPositions.Last().OpenedAt <= candle.Date.AddSeconds(-4 * timeFrame))
            await _broker.CancelAllShortPendingPositions();

        bool shouldOpenLongPosition = false;
        bool shouldOpenShortPosition = false;
        if (!_strategyOptions.InvalidWeekDays.Any() || (_strategyOptions.InvalidWeekDays.Any() && !_strategyOptions.InvalidWeekDays.Where(invalidDate => candleCloseDate.DayOfWeek == invalidDate).Any()))
            if (!_strategyOptions.InvalidTimePeriods.Any() || (_strategyOptions.InvalidTimePeriods.Any() && !_strategyOptions.InvalidTimePeriods.Where(invalidTimePeriod => candleCloseDate.TimeOfDay <= invalidTimePeriod.End.TimeOfDay && candleCloseDate.TimeOfDay >= invalidTimePeriod.Start.TimeOfDay).Any()))
                if (!_strategyOptions.InvalidDatePeriods.Any() || (_strategyOptions.InvalidDatePeriods.Any() && !_strategyOptions.InvalidDatePeriods.Where(invalidDatePeriod => candleCloseDate.Date <= invalidDatePeriod.End.Date && candleCloseDate.Date >= invalidDatePeriod.Start.Date).Any()))
                {
                    // if (openPositions.Any())
                    //     return;

                    if (!longPendingPositions.Any())
                        shouldOpenLongPosition = true;

                    if (!shortPendingPositions.Any())
                        shouldOpenShortPosition = true;
                }

        decimal offset = Math.Abs(candle.Close - candle.Open) / 4.0m;

        decimal upperBand = Math.Max(candle.Close, candle.Open) + offset;
        decimal lowerBand = Math.Min(candle.Close, candle.Open) - offset;

        decimal delta = Math.Abs(upperBand - lowerBand);

        // if ((double)delta < _deltaWma.ElementAt(index).Wma)
        //     return;

        if ((double)delta < 50)
            return;

        decimal longTpPrice = upperBand + (delta * _strategyOptions.RiskRewardRatio);
        decimal longSlPrice = lowerBand;

        decimal shortTpPrice = lowerBand - (delta * _strategyOptions.RiskRewardRatio);
        decimal shortSlPrice = upperBand;

        if (shouldOpenLongPosition)
        {
            _logger.Information("Candle is valid for a position, sending the message...");

            if (IsLong(index))
                if (candle.Close > candle.Open)
                {
                    IMessage longMessage = CreateOpenPositionMessage(candle, timeFrame, PositionDirection.LONG, upperBand, longSlPrice, longTpPrice);
                    await _messageRepository.CreateMessage(longMessage);
                }

            _logger.Information("Messages sent.");
        }

        if (shouldOpenShortPosition)
        {
            _logger.Information("Candle is valid for a position, sending the message...");

            if (IsShort(index))
                if (candle.Close < candle.Open)
                {
                    IMessage shortMessage = CreateOpenPositionMessage(candle, timeFrame, PositionDirection.SHORT, lowerBand, shortSlPrice, shortTpPrice);
                    await _messageRepository.CreateMessage(shortMessage);
                }

            _logger.Information("Messages sent.");
        }
    }

    private bool IsLong(int index) => true;

    private bool IsShort(int index) => false;

    private IMessage CreateOpenPositionMessage(Candle candle, int timeFrame, string direction, decimal limit, decimal slPrice, decimal tpPrice) => new Message()
    {
        From = _strategyOptions.ProviderName,
        Body = Bots.CandlesOpenClose.Models.Message.CreateMessageBody(
            openingPosition: true,
            allowingParallelPositions: true,
            closingAllPositions: false,
            direction,
            limit,
            slPrice,
            tpPrice
        ),
        SentAt = candle.Date.AddSeconds(timeFrame)
    };
}