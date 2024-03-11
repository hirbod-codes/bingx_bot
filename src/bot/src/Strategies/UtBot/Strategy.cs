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

public class Strategy : IStrategy
{
    private readonly IndicatorOptions _indicatorsOptions;
    private readonly IMessageRepository _messageRepository;
    private readonly StrategyOptions _strategyOptions;
    private readonly IBroker _broker;
    private readonly ILogger _logger;
    private IEnumerable<AtrResult> _atr = null!;
    private IEnumerable<AtrStopResult> _atrStop = Array.Empty<AtrStopResult>();
    private IEnumerable<EmaResult> _ema = Array.Empty<EmaResult>();

    public Strategy(IStrategyOptions strategyOptions, IIndicatorOptions indicatorOptions, IMessageRepository messageRepository, IBroker broker, ILogger logger)
    {
        _strategyOptions = (strategyOptions as StrategyOptions)!;
        _indicatorsOptions = (indicatorOptions as IndicatorOptions)!;
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
        if (candlesCount <= _indicatorsOptions.AtrStopPeriod.Period + 1 || candlesCount <= _indicatorsOptions.EmaPeriod.Period)
            throw new NotEnoughCandlesException();

        _atr = candles.GetAtr(_indicatorsOptions.AtrPeriod.Period);
        _atrStop = candles.GetAtrStop(_indicatorsOptions.AtrStopPeriod.Period, _indicatorsOptions.AtrStopMultiplier);
        _ema = candles.GetEma(_indicatorsOptions.EmaPeriod.Period);

        if (_atrStop.Count() != candlesCount || _ema.Count() != candlesCount)
            throw new StrategyException();

        _logger.Information("Indicators created...");
    }

    public async Task HandleCandle(Candle candle, int timeFrame)
    {
        _logger.Information("Handling the candle...");
        _logger.Information("candle: {@candle}", candle);
        _logger.Information("Time Frame: {@timeFrame}", timeFrame);

        int index = await _broker.GetLastCandleIndex();

        Signal(candle, index, out bool buy, out bool sell);

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
            decimal slPrice = CalculateSlPrice(index, candle.Close, buy);

            IMessage message = CreateOpenPositionMessage(candle, timeFrame, buy ? PositionDirection.LONG : PositionDirection.SHORT, slPrice, null);
            _logger.Information("Message:  {@message}.", message);

            await _messageRepository.CreateMessage(message);
            _logger.Information("Message sent.");
        }

        _logger.Information("Finished handling the candle...");
    }

    private decimal CalculateSlPrice(int index, decimal entryPrice, bool buy) =>
        buy ? entryPrice - (decimal)(_atr.ElementAt(index).Atr * _indicatorsOptions.AtrMultiplier)! : entryPrice + (decimal)(_atr.ElementAt(index).Atr * _indicatorsOptions.AtrMultiplier)!;

    private void Signal(Candle candle, int index, out bool buy, out bool sell)
    {
        bool above = HasCrossedOver(_ema, _atrStop, index);
        bool below = HasCrossedUnder(_ema, _atrStop, index);

        buy = (double)candle.Close > (double)_atrStop.ElementAt(index).AtrStop! && above;
        sell = (double)candle.Close < (double)_atrStop.ElementAt(index).AtrStop! && below;

        _logger.Information("Signal1 ==> above: {above}, below: {below}, buy: {buy}, sell: {sell}", above, below, buy, sell);
    }

    public bool HasCrossedOver(IEnumerable<IReusableResult> ema, IEnumerable<AtrStopResult> atrStop, int index)
    {
        double previousEma = (double)ema.ElementAt(index - 1).Value!;
        double lastEma = (double)ema.ElementAt(index).Value!;
        double previousAtrStop = (double)atrStop.ElementAt(index - 1).AtrStop!;
        double lastAtrStop = (double)atrStop.ElementAt(index).AtrStop!;

        _logger.Information("previousEma: {previousEma}, lastEma: {lastEma}, previousAtrStop: {previousAtrStop}, lastAtrStop: {lastAtrStop}", previousEma, lastEma, previousAtrStop, lastAtrStop);

        return previousEma <= previousAtrStop && lastEma > lastAtrStop;
    }

    public bool HasCrossedUnder(IEnumerable<IReusableResult> ema, IEnumerable<AtrStopResult> atrStop, int index)
    {
        double previousEma = (double)ema.ElementAt(index - 1).Value!;
        double lastEma = (double)ema.ElementAt(index).Value!;
        double previousAtrStop = (double)atrStop.ElementAt(index - 1).AtrStop!;
        double lastAtrStop = (double)atrStop.ElementAt(index).AtrStop!;

        _logger.Information("previousEma: {previousEma}, lastEma: {lastEma}, previousAtrStop: {previousAtrStop}, lastAtrStop: {lastAtrStop}", previousEma, lastEma, previousAtrStop, lastAtrStop);
        return previousEma >= previousAtrStop && lastEma < lastAtrStop;
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
