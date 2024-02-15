using bot.src.Bots.UtBot.Models;
using bot.src.Brokers;
using bot.src.Data;
using bot.src.Data.Models;
using bot.src.Indicators;
using bot.src.Indicators.UtBot;
using bot.src.MessageStores;
using bot.src.MessageStores.InMemory.Models;
using Serilog;
using Skender.Stock.Indicators;

namespace bot.src.Strategies.UtBot;

public class UtBotStrategy : IStrategy
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

    public UtBotStrategy(IStrategyOptions strategyOptions, IIndicatorOptions indicatorOptions, IMessageRepository messageRepository, IBroker broker, ILogger logger)
    {
        _strategyOptions = (strategyOptions as StrategyOptions)!;
        _indicatorOptions = (indicatorOptions as IndicatorOptions)!;
        _messageRepository = messageRepository;
        _broker = broker;
        _logger = logger.ForContext<UtBotStrategy>();
    }

    private void CreateIndicators(Candles candles)
    {
        _logger.Information("Creating indicators...");

        _atrStop1 = candles.GetAtrStop(_indicatorOptions.AtrPeriod1.Period, _indicatorOptions.AtrMultiplier1);
        _ema1 = candles.GetEma(_indicatorOptions.EmaPeriod1.Period);

        _atrStop2 = candles.GetAtrStop(_indicatorOptions.AtrPeriod2.Period, _indicatorOptions.AtrMultiplier2);
        _ema2 = candles.GetEma(_indicatorOptions.EmaPeriod2.Period);

        _logger.Information("Indicators created...");
    }

    public async Task HandleCandle(Candle candle, int timeFrame)
    {
        _logger.Information("Handling the candle...");
        _logger.Information("candle: {@candle}", candle);
        _logger.Information("Time Frame: {@timeFrame}", timeFrame);

        CreateIndicators(await _broker.GetCandles());

        Signal1(candle, out bool buy1, out bool sell1);
        Signal2(candle, out bool buy2, out bool sell2);

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

    private void Signal1(Candle candle, out bool buy, out bool sell)
    {
        bool above = HasCrossedOver(_ema1, _atrStop1);
        bool below = HasCrossedUnder(_ema1, _atrStop1);

        buy = (double)candle.Close > (double)_atrStop1.Last().AtrStop! && above;
        sell = (double)candle.Close < (double)_atrStop1.Last().AtrStop! && below;

        _logger.Information("Signal1 ==> above: {above}, below: {below}, buy: {buy}, sell: {sell}", above, below, buy, sell);
    }

    private void Signal2(Candle candle, out bool buy, out bool sell)
    {
        bool above = HasCrossedOver(_ema2, _atrStop2);
        bool below = HasCrossedUnder(_ema2, _atrStop2);

        buy = (double)candle.Close > (double)_atrStop2.Last().AtrStop! && above;
        sell = (double)candle.Close < (double)_atrStop2.Last().AtrStop! && below;

        _logger.Information("Signal2 ==> above: {above}, below: {below}, buy: {buy}, sell: {sell}", above, below, buy, sell);
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
    public bool HasCrossedOver(IEnumerable<IReusableResult> ema, IEnumerable<AtrStopResult> atrStop)
    {
        double previousEma = (double)ema.ElementAt(ema.Count() - 2).Value!;
        double lastEma = (double)ema.Last().Value!;
        double previousAtrStop = (double)atrStop.ElementAt(atrStop.Count() - 2).AtrStop!;
        double lastAtrStop = (double)atrStop.Last().AtrStop!;

        _logger.Information("previousEma: {previousEma}, lastEma: {lastEma}, previousAtrStop: {previousAtrStop}, lastAtrStop: {lastAtrStop}", previousEma, lastEma, previousAtrStop, lastAtrStop);

        return previousEma <= previousAtrStop && lastEma > lastAtrStop;
    }

    public bool HasCrossedUnder(IEnumerable<IReusableResult> ema, IEnumerable<AtrStopResult> atrStop)
    {
        double previousEma = (double)ema.ElementAt(ema.Count() - 2).Value!;
        double lastEma = (double)ema.Last().Value!;
        double previousAtrStop = (double)atrStop.ElementAt(atrStop.Count() - 2).AtrStop!;
        double lastAtrStop = (double)atrStop.Last().AtrStop!;

        _logger.Information("previousEma: {previousEma}, lastEma: {lastEma}, previousAtrStop: {previousAtrStop}, lastAtrStop: {lastAtrStop}", previousEma, lastEma, previousAtrStop, lastAtrStop);
        return previousEma >= previousAtrStop && lastEma < lastAtrStop;
    }
}
