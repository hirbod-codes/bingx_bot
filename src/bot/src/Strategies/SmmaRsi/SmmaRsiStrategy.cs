using bot.src.Data;
using bot.src.Data.Models;
using bot.src.MessageStores;
using bot.src.MessageStores.InMemory.Models;
using bot.src.Notifiers;
using bot.src.RiskManagement;
using Serilog;
using Skender.Stock.Indicators;
using bot.src.Indicators;
using bot.src.Indicators.SmmaRsi;
using bot.src.Strategies.SmmaRsi.Exceptions;

namespace bot.src.Strategies.SmmaRsi;

public class SmmaRsiStrategy : IStrategy
{
    private readonly ICandleRepository _candleRepository;
    private readonly IndicatorsOptions _indicatorsOptions;
    private readonly StrategyOptions _strategyOptions;
    private IEnumerable<SmmaResult> _smma1 = null!;
    private IEnumerable<SmmaResult> _smma2 = null!;
    private IEnumerable<SmmaResult> _smma3 = null!;
    private IEnumerable<RsiResult> _rsi = null!;
    private readonly INotifier _notifier;
    private readonly IRiskManagement _riskManagement;
    private readonly ILogger _logger;

    public SmmaRsiStrategy(ICandleRepository candleRepository, IStrategyOptions strategyOptions, IIndicatorsOptions indicatorsOptions, INotifier notifier, IRiskManagement riskManagement, ILogger logger)
    {
        _candleRepository = candleRepository;
        _indicatorsOptions = (indicatorsOptions as IndicatorsOptions)!;
        _strategyOptions = (strategyOptions as StrategyOptions)!;
        _notifier = notifier;
        _riskManagement = riskManagement;

        _logger = logger.ForContext<SmmaRsiStrategy>();
    }

    public async Task Initialize()
    {
        Candles candles = await _candleRepository.GetCandles();
        _smma1 = candles.GetSmma(_indicatorsOptions.Smma1.Period);
    }

    public async Task HandleCandle(Candle candle, int index)
    {
        if (_smma1 == null && _smma2 == null && _smma3 == null && _rsi == null)
            throw new NoIndicatorException();
        bool isUpTrend = IsUpTrend(index);
        bool isDownTrend = IsDownTrend(index);
        bool isInTrend = isUpTrend || isDownTrend;

        bool rsiCrossedOverLowerBand = HasRsiCrossedOverLowerBand(index);
        bool rsiCrossedUnderUpperBand = HasRsiCrossedUnderUpperBand(index);

        bool shouldOpenPosition = false;
        if (isInTrend)
            if ((isUpTrend && rsiCrossedOverLowerBand) || (isDownTrend && rsiCrossedUnderUpperBand))
                shouldOpenPosition = true;

        if (shouldOpenPosition)
        {
            _logger.Information("Candle is valid for a position, sending the message...");

            IMessage message = await CreateOpenPositionMessage(candle, isUpTrend ? PositionDirection.LONG : PositionDirection.SHORT, hasTPPrice: true);
            await _notifier.SendMessage(message);
            _logger.Information("Message sent.");
        }
    }

    private bool HasRsiCrossedUnderUpperBand(int index)
    {
        index = _rsi.Count() - 1 - index;

        if (_rsi.ElementAt(index).Rsi is null || _rsi.ElementAt(index - 1).Rsi is null)
            return false;

        return _rsi.ElementAt(index - 1).Rsi > _indicatorsOptions.Rsi.UpperBand && _rsi.ElementAt(index).Rsi < _indicatorsOptions.Rsi.UpperBand;
    }

    private bool HasRsiCrossedOverLowerBand(int index)
    {
        index = _rsi.Count() - 1 - index;

        if (_rsi.ElementAt(index).Rsi is null || _rsi.ElementAt(index - 1).Rsi is null)
            return false;

        return _rsi.ElementAt(index - 1).Rsi < _indicatorsOptions.Rsi.LowerBand && _rsi.ElementAt(index).Rsi > _indicatorsOptions.Rsi.LowerBand;
    }

    private bool IsDownTrend(int index)
    {
        index = _smma1.Count() - 1 - index;

        if (_smma1.ElementAt(index).Smma is null || _smma2.ElementAt(index).Smma is null || _smma3.ElementAt(index).Smma is null)
            return false;

        return _smma1.ElementAt(index).Smma <= _smma2.ElementAt(index).Smma && _smma2.ElementAt(index).Smma <= _smma3.ElementAt(index).Smma;
    }

    private bool IsUpTrend(int index)
    {
        index = _smma1.Count() - 1 - index;

        if (_smma1.ElementAt(index).Smma is null || _smma2.ElementAt(index).Smma is null || _smma3.ElementAt(index).Smma is null)
            return false;

        return _smma1.ElementAt(index).Smma >= _smma2.ElementAt(index).Smma && _smma2.ElementAt(index).Smma >= _smma3.ElementAt(index).Smma;
    }

    private async Task<IMessage> CreateOpenPositionMessage(Candle candle, string direction, bool hasTPPrice)
    {
        string message = IGeneralMessage.CreateMessageBody(
            openingPosition: true,
            allowingParallelPositions: true,
            closingAllPositions: false,
            direction,
            _riskManagement.GetLeverage(),
            _riskManagement.GetMargin(),
            await _candleRepository.GetTimeFrame(),
            _riskManagement.GetSLPrice(direction, candle.Close),
            hasTPPrice ? _riskManagement.GetTPPrice(direction, candle.Close) : null
        );

        return new Message()
        {
            From = nameof(SmmaRsiStrategy),
            Body = message,
            SentAt = candle.Date.AddSeconds(await _candleRepository.GetTimeFrame())
        };
    }
}
