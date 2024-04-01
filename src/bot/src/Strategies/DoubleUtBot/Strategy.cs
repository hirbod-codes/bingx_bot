using bot.src.Bots.DoubleUtBot.Models;
using bot.src.Brokers;
using bot.src.Data;
using bot.src.Data.Models;
using bot.src.Indicators;
using bot.src.Indicators.DoubleUtBot;
using bot.src.MessageStores;
using bot.src.MessageStores.InMemory.Models;
using Serilog;
using Skender.Stock.Indicators;

namespace bot.src.Strategies.DoubleUtBot;

public class Strategy : IStrategy
{
    private readonly IndicatorOptions _indicatorOptions;
    private readonly IMessageRepository _messageRepository;
    private readonly StrategyOptions _strategyOptions;
    private readonly IBroker _broker;
    private readonly ILogger _logger;
    private IEnumerable<AtrStopResult> _atrStop1 = Array.Empty<AtrStopResult>();
    private IEnumerable<EmaResult> _ema1 = Array.Empty<EmaResult>();
    private IEnumerable<AtrStopResult> _atrStop2 = Array.Empty<AtrStopResult>();
    private IEnumerable<EmaResult> _ema2 = Array.Empty<EmaResult>();

    public Strategy(IStrategyOptions strategyOptions, IIndicatorOptions indicatorOptions, IMessageRepository messageRepository, IBroker broker, ILogger logger)
    {
        _strategyOptions = (strategyOptions as StrategyOptions)!;
        _indicatorOptions = (indicatorOptions as IndicatorOptions)!;
        _messageRepository = messageRepository;
        _broker = broker;
        _logger = logger.ForContext<Strategy>();
    }

    public async Task PrepareIndicators()
    {
        _logger.Information("Creating indicators...");

        Candles candles = await _broker.GetCandles();
        int candlesCount = candles.Count();

        // adding one to the atr stop indicator because this strategy needs previous values of this indicator.
        if (candlesCount <= _indicatorOptions.AtrPeriod1.Period + 1 || candlesCount <= _indicatorOptions.EmaPeriod1.Period || candlesCount <= _indicatorOptions.AtrPeriod2.Period + 1 || candlesCount <= _indicatorOptions.EmaPeriod2.Period)
            throw new NotEnoughCandlesException();

        _atrStop1 = candles.GetAtrStop(_indicatorOptions.AtrPeriod1.Period, _indicatorOptions.AtrMultiplier1);
        _ema1 = candles.GetEma(_indicatorOptions.EmaPeriod1.Period);

        _atrStop2 = candles.GetAtrStop(_indicatorOptions.AtrPeriod2.Period, _indicatorOptions.AtrMultiplier2);
        _ema2 = candles.GetEma(_indicatorOptions.EmaPeriod2.Period);

        if (_atrStop1.Count() != candlesCount || _ema1.Count() != candlesCount || _atrStop2.Count() != candlesCount || _ema2.Count() != candlesCount)
            throw new StrategyException();

        _logger.Information("Indicators created...");
    }

    public Dictionary<string, object> GetIndicators() => new(new KeyValuePair<string, object>[]{
            new(nameof(_atrStop1), _atrStop1),
            new(nameof(_ema1), _ema1),
            new(nameof(_atrStop2), _atrStop2),
            new(nameof(_ema2), _ema2)
        });

    public async Task HandleCandle(Candle candle, int timeFrame)
    {
        _logger.Information("Handling the candle...");
        _logger.Information("candle: {@candle}", candle);
        _logger.Information("Time Frame: {@timeFrame}", timeFrame);

        int index = (int)await _broker.GetLastCandleIndex();

        Signal1(candle, index, out bool buy1, out bool sell1);
        Signal2(candle, index, out bool buy2, out bool sell2);

        bool buy = buy1 || buy2;
        bool sell = sell1 || sell2;

        _logger.Information("buy: {buy}", buy);
        _logger.Information("sell: {sell}", sell);

        DateTime candleCloseDate = candle.Date.AddSeconds(timeFrame);

        _logger.Information("Candle close date: {candleCloseDate}", candleCloseDate);

        bool shouldOpenPosition = false;

        if (buy || sell)
            if (!_strategyOptions.InvalidWeekDays.Any() || (_strategyOptions.InvalidWeekDays.Any() && !_strategyOptions.InvalidWeekDays.Where(invalidDate => candleCloseDate.DayOfWeek == invalidDate).Any()))
                if (!_strategyOptions.InvalidTimePeriods.Any() || (_strategyOptions.InvalidTimePeriods.Any() && !_strategyOptions.InvalidTimePeriods.Where(invalidTimePeriod => candleCloseDate.TimeOfDay <= invalidTimePeriod.End.TimeOfDay && candleCloseDate.TimeOfDay >= invalidTimePeriod.Start.TimeOfDay).Any()))
                    if (!_strategyOptions.InvalidDatePeriods.Any() || (_strategyOptions.InvalidDatePeriods.Any() && !_strategyOptions.InvalidDatePeriods.Where(invalidDatePeriod => candleCloseDate.Date <= invalidDatePeriod.End.Date && candleCloseDate.Date >= invalidDatePeriod.Start.Date).Any()))
                        shouldOpenPosition = true;

        _logger.Information("Should open position: {shouldOpenPosition}", shouldOpenPosition);

        if (shouldOpenPosition)
        {
            IMessage message = CreateOpenPositionMessage(candle, timeFrame, buy ? PositionDirection.LONG : PositionDirection.SHORT, buy ? candle.Close - _strategyOptions.SLDifference : candle.Close + _strategyOptions.SLDifference, null);
            _logger.Information("Message:  {@message}.", message);

            await _messageRepository.CreateMessage(message);
            _logger.Information("Message sent.");
        }

        _logger.Information("Finished handling the candle...");
    }

    private void Signal1(Candle candle, int index, out bool buy, out bool sell)
    {
        bool above = HasCrossedOver(_ema1, _atrStop1, index);
        bool below = HasCrossedUnder(_ema1, _atrStop1, index);

        buy = (double)candle.Close > (double)_atrStop1.ElementAt(index).AtrStop! && above;
        sell = (double)candle.Close < (double)_atrStop1.ElementAt(index).AtrStop! && below;

        _logger.Information("Signal1 ==> above: {above}, below: {below}, buy: {buy}, sell: {sell}", above, below, buy, sell);
    }

    private void Signal2(Candle candle, int index, out bool buy, out bool sell)
    {
        bool above = HasCrossedOver(_ema2, _atrStop2, index);
        bool below = HasCrossedUnder(_ema2, _atrStop2, index);

        buy = (double)candle.Close > (double)_atrStop2.ElementAt(index).AtrStop! && above;
        sell = (double)candle.Close < (double)_atrStop2.ElementAt(index).AtrStop! && below;

        _logger.Information("Signal2 ==> above: {above}, below: {below}, buy: {buy}, sell: {sell}", above, below, buy, sell);
    }

    public bool HasCrossedOver(IEnumerable<IReusableResult> ema, IEnumerable<AtrStopResult> atrStop, int index)
    {
        try
        {
            double previousEma = (double)ema.ElementAt(index - 1).Value!;
            double lastEma = (double)ema.ElementAt(index).Value!;
            double previousAtrStop = (double)atrStop.ElementAt(index - 1).AtrStop!;
            double lastAtrStop = (double)atrStop.ElementAt(index).AtrStop!;

            _logger.Information("previousEma: {previousEma}, lastEma: {lastEma}, previousAtrStop: {previousAtrStop}, lastAtrStop: {lastAtrStop}", previousEma, lastEma, previousAtrStop, lastAtrStop);

            return previousEma <= previousAtrStop && lastEma > lastAtrStop;
        }
        catch (Exception)
        {
            Candle candle = _broker.GetCandle().GetAwaiter().GetResult();
            int atrStopCount = atrStop.Count();
            int emaCount = ema.Count();
            int _atrStop1Count = _atrStop1.Count();
            int _ema1Count = _ema1.Count();
            int _atrStop2Count = _atrStop2.Count();
            int _ema2Count = _ema2.Count();

            AtrStopResult atrStopLast = atrStop.ElementAt(index);
            IReusableResult emaLast = ema.ElementAt(index);
            AtrStopResult _atrStop1Last = _atrStop1.ElementAt(index);
            EmaResult _ema1Last = _ema1.ElementAt(index);
            AtrStopResult _atrStop2Last = _atrStop2.ElementAt(index);
            EmaResult _ema2Last = _ema2.ElementAt(index);
            throw;
        }
    }

    public bool HasCrossedUnder(IEnumerable<IReusableResult> ema, IEnumerable<AtrStopResult> atrStop, int index)
    {
        try
        {
            double previousEma = (double)ema.ElementAt(index - 1).Value!;
            double lastEma = (double)ema.ElementAt(index).Value!;
            double previousAtrStop = (double)atrStop.ElementAt(index - 1).AtrStop!;
            double lastAtrStop = (double)atrStop.ElementAt(index).AtrStop!;

            _logger.Information("previousEma: {previousEma}, lastEma: {lastEma}, previousAtrStop: {previousAtrStop}, lastAtrStop: {lastAtrStop}", previousEma, lastEma, previousAtrStop, lastAtrStop);
            return previousEma >= previousAtrStop && lastEma < lastAtrStop;
        }
        catch (Exception)
        {
            Candle candle = _broker.GetCandle().GetAwaiter().GetResult();
            int atrStopCount = atrStop.Count();
            int emaCount = ema.Count();
            int _atrStop1Count = _atrStop1.Count();
            int _ema1Count = _ema1.Count();
            int _atrStop2Count = _atrStop2.Count();
            int _ema2Count = _ema2.Count();

            AtrStopResult atrStopLast = atrStop.ElementAt(index);
            IReusableResult emaLast = ema.ElementAt(index);
            AtrStopResult _atrStop1Last = _atrStop1.ElementAt(index);
            EmaResult _ema1Last = _ema1.ElementAt(index);
            AtrStopResult _atrStop2Last = _atrStop2.ElementAt(index);
            EmaResult _ema2Last = _ema2.ElementAt(index);
            throw;
        }
    }

    private IMessage CreateOpenPositionMessage(Candle candle, int timeFrame, string direction, decimal slPrice, decimal? tpPrice) => new Message()
    {
        From = _strategyOptions.ProviderName,
        Body = IUtBotMessage.CreateMessageBody(
                openingPosition: true,
                direction,
                slPrice
            ),
        SentAt = candle.Date.AddSeconds(timeFrame)
    };
}
