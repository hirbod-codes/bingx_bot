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
using Bogus.DataSets;

namespace bot.src.Strategies.CandlesOpenClose;

public class Strategy : IStrategy
{
    private readonly IndicatorOptions _indicatorsOptions;
    private readonly StrategyOptions _strategyOptions;
    private IEnumerable<WmaResult> _wma1 = Array.Empty<WmaResult>();
    private IEnumerable<WmaResult> _wma2 = Array.Empty<WmaResult>();
    private IEnumerable<RsiResult> _rsi = Array.Empty<RsiResult>();
    private IEnumerable<AtrResult> _atr = Array.Empty<AtrResult>();
    private IEnumerable<StochResult> _stochastic = Array.Empty<StochResult>();
    private IEnumerable<SuperTrendResult> _superTrend = Array.Empty<SuperTrendResult>();
    private IEnumerable<DeltaResult> _delta = Array.Empty<DeltaResult>();
    private IEnumerable<WmaResult> _deltaWma = Array.Empty<WmaResult>();
    private IEnumerable<RsiResult> _deltaRsi = Array.Empty<RsiResult>();
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
            new(nameof(_wma1), _wma1),
            new(nameof(_wma2), _wma2),
            new(nameof(_rsi), _rsi),
            new(nameof(_atr), _atr),
            new(nameof(_stochastic), _stochastic),
            new(nameof(_superTrend), _superTrend),
            new(nameof(_deltaWma), _deltaWma),
            new(nameof(_deltaRsi), _deltaRsi),
            new(nameof(_delta), _delta)
        });

    public async Task PrepareIndicators()
    {
        Candles candles = await _broker.GetCandles() ?? throw new CandlesNotFoundException();

        _stochastic = candles.GetStoch(_indicatorsOptions.Stochastic.Period, _indicatorsOptions.Stochastic.SignalPeriod, _indicatorsOptions.Stochastic.SmoothPeriod);
        _superTrend = candles.GetSuperTrend(10, 2);
        _atr = candles.GetAtr();
        _rsi = candles.GetRsi();
        _wma1 = candles.GetWma(10);
        _wma2 = candles.GetWma(50);

        _delta = candles.ToList().ConvertAll(candle =>
        {
            decimal offset = Math.Abs(candle.Close - candle.Open) / 4.0m;

            decimal upperBand = Math.Max(candle.Close, candle.Open) + offset;
            decimal lowerBand = Math.Min(candle.Close, candle.Open) - offset;

            decimal delta = Math.Abs(upperBand - lowerBand);

            return new DeltaResult(candle.Date) { Delta = delta };
        });

        _deltaWma = candles.ToList().ConvertAll(candle =>
        {
            decimal offset = Math.Abs(candle.Close - candle.Open) / 4.0m;

            decimal upperBand = Math.Max(candle.Close, candle.Open) + offset;
            decimal lowerBand = Math.Min(candle.Close, candle.Open) - offset;

            decimal delta = Math.Abs(upperBand - lowerBand);

            return new Candle() { Date = candle.Date, Close = delta };
        }).GetWma(20);

        _deltaRsi = candles.ToList().ConvertAll(candle =>
        {
            decimal offset = Math.Abs(candle.Close - candle.Open) / 4.0m;

            decimal upperBand = Math.Max(candle.Close, candle.Open) + offset;
            decimal lowerBand = Math.Min(candle.Close, candle.Open) - offset;

            decimal delta = Math.Abs(upperBand - lowerBand);

            return new Candle() { Date = candle.Date, Close = delta };
        }).GetRsi();
    }

    public async Task HandleCandle(Candle candle, int timeFrame)
    {
        int index = (int)await _broker.GetLastCandleIndex();

        DateTime candleCloseDate = candle.Date.AddSeconds(timeFrame);

        IEnumerable<Position> openPositions = (await _broker.GetOpenPositions()).Where(o => o != null)!;
        IEnumerable<Position> pendingPositions = (await _broker.GetPendingPositions()).Where(o => o != null)!;
        IEnumerable<Position> longPendingPositions = pendingPositions.Where(o => o.PositionDirection == PositionDirection.LONG)!;
        IEnumerable<Position> shortPendingPositions = pendingPositions.Where(o => o.PositionDirection == PositionDirection.SHORT)!;

        if (longPendingPositions.Any() && longPendingPositions.Last().CreatedAt <= candle.Date.AddSeconds(-4 * timeFrame))
            await _broker.CancelAllLongPendingPositions();

        if (shortPendingPositions.Any() && shortPendingPositions.Last().CreatedAt <= candle.Date.AddSeconds(-4 * timeFrame))
            await _broker.CancelAllShortPendingPositions();

        bool shouldOpenLongPosition = false;
        bool shouldOpenShortPosition = false;
        if (!_strategyOptions.InvalidWeekDays.Any() || (_strategyOptions.InvalidWeekDays.Any() && !_strategyOptions.InvalidWeekDays.Where(invalidDate => candleCloseDate.DayOfWeek == invalidDate).Any()))
            if (!_strategyOptions.InvalidTimePeriods.Any() || (_strategyOptions.InvalidTimePeriods.Any() && !_strategyOptions.InvalidTimePeriods.Where(invalidTimePeriod => candleCloseDate.TimeOfDay <= invalidTimePeriod.End.TimeOfDay && candleCloseDate.TimeOfDay >= invalidTimePeriod.Start.TimeOfDay).Any()))
                if (!_strategyOptions.InvalidDatePeriods.Any() || (_strategyOptions.InvalidDatePeriods.Any() && !_strategyOptions.InvalidDatePeriods.Where(invalidDatePeriod => candleCloseDate.Date <= invalidDatePeriod.End.Date && candleCloseDate.Date >= invalidDatePeriod.Start.Date).Any()))
                {
                    if (openPositions.Any())
                    {
                        if (ShouldCloseAll(index))
                        {
                            _logger.Information("Closing all the open positions...");

                            IMessage longMessage = CreateClosePositionsMessage(candle, timeFrame);
                            await _messageRepository.CreateMessage(longMessage);

                            _logger.Information("Messages sent.");
                        }
                        return;
                    }

                    if (!longPendingPositions.Any())
                        shouldOpenLongPosition = true;

                    if (!shortPendingPositions.Any())
                        shouldOpenShortPosition = true;
                }

        decimal offset = Math.Abs(candle.Close - candle.Open) / 4.0m;

        decimal upperBand = Math.Max(candle.Close, candle.Open) + offset;
        decimal lowerBand = Math.Min(candle.Close, candle.Open) - offset;

        decimal delta = Math.Abs(upperBand - lowerBand);

        if ((double)delta < 50)
            return;

        if (_deltaRsi.ElementAt(index).Rsi > 70 || _deltaRsi.ElementAt(index).Rsi < 30)
            return;

        decimal longTpPrice = upperBand + (delta * _strategyOptions.RiskRewardRatio);
        decimal longSlPrice = lowerBand;

        decimal shortTpPrice = lowerBand - (delta * _strategyOptions.RiskRewardRatio);
        decimal shortSlPrice = upperBand;

        if (shouldOpenLongPosition)
            if (IsLong(index))
            // if (candle.Close > candle.Open)
            {
                _logger.Information("Candle is valid for a position, sending the message...");

                IMessage longMessage = CreateOpenPositionMessage(candle, timeFrame, PositionDirection.LONG, upperBand, longSlPrice, longTpPrice);
                await _messageRepository.CreateMessage(longMessage);

                _logger.Information("Messages sent.");
            }

        if (shouldOpenShortPosition)
            if (IsShort(index))
            // if (candle.Close < candle.Open)
            {
                _logger.Information("Candle is valid for a position, sending the message...");

                IMessage shortMessage = CreateOpenPositionMessage(candle, timeFrame, PositionDirection.SHORT, lowerBand, shortSlPrice, shortTpPrice);
                await _messageRepository.CreateMessage(shortMessage);

                _logger.Information("Messages sent.");
            }
    }

    private bool ShouldCloseAll(int index) => false;
    // private bool ShouldCloseAll(int index) => _rsi.ElementAt(index - 1).Rsi > 70 && _rsi.ElementAt(index).Rsi < 70;

    private IMessage CreateClosePositionsMessage(Candle candle, int timeFrame) => new Message()
    {
        From = _strategyOptions.ProviderName,
        Body = Bots.CandlesOpenClose.Models.Message.CreateMessageBody(
            openingPosition: false,
            allowingParallelPositions: false,
            closingAllPositions: true,
            PositionDirection.LONG,
            0,
            0
        ),
        SentAt = candle.Date.AddSeconds(timeFrame)
    };

    private bool IsLong(int index) => true;
    // private bool IsLong(int index) => _wma1.ElementAt(index).Wma > _wma2.ElementAt(index).Wma;
    // private bool IsLong(int index) => _superTrend.ElementAt(index).LowerBand != null && _rsi.ElementAt(index).Rsi > 40 && _rsi.ElementAt(index).Rsi < 65;

    private bool IsShort(int index) => false;
    // private bool IsShort(int index) => _rsi.ElementAt(index).Rsi < 60 && _rsi.ElementAt(index).Rsi > 35;
    // private bool IsShort(int index) => _superTrend.ElementAt(index).UpperBand != null;

    private IMessage CreateOpenPositionMessage(Candle candle, int timeFrame, string direction, decimal limit, decimal slPrice, decimal? tpPrice = null) => new Message()
    {
        From = _strategyOptions.ProviderName,
        Body = Bots.CandlesOpenClose.Models.Message.CreateMessageBody(
            openingPosition: true,
            allowingParallelPositions: false,
            closingAllPositions: false,
            direction,
            limit,
            slPrice,
            tpPrice
        ),
        SentAt = candle.Date.AddSeconds(timeFrame)
    };

    private class DeltaResult : ResultBase
    {
        public DeltaResult(DateTime date) => base.Date = date;

        public decimal Delta { get; set; }
    }
}
