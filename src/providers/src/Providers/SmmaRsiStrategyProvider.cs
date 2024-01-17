using bot.src.Data;
using bot.src.Data.Models;
using bot.src.MessageStores;
using bot.src.MessageStores.InMemory.Models;
using bot.src.Notifiers;
using bot.src.RiskManagement;
using providers.src.IndicatorOptions;
using Serilog;
using Skender.Stock.Indicators;

namespace providers.src.Providers;

public class SmmaRsiStrategyProvider : IStrategyProvider
{
    private readonly ICandleRepository _candleRepository;
    private readonly IndicatorsOptions _indicatorsOptions;
    private readonly IEnumerable<SmmaResult> _smma1;
    private readonly IEnumerable<SmmaResult> _smma2;
    private readonly IEnumerable<SmmaResult> _smma3;
    private readonly IEnumerable<RsiResult> _rsi;
    private readonly INotifier _notifier;
    private readonly IRiskManagement _riskManagement;
    private readonly ILogger _logger;
    private readonly ITime _time;
    private int? _index = null;
    private bool _hasBrokerProcessedCandle = false;
    private bool _hasBotTicked = false;

    public event EventHandler<OnCandleCloseEventsArgs>? CandleClosed;
    public event EventHandler? LastCandleReached;

    public SmmaRsiStrategyProvider(ICandleRepository candleRepository, IndicatorsOptions indicatorsOptions, IEnumerable<SmmaResult> smma1, IEnumerable<SmmaResult> smma2, IEnumerable<SmmaResult> smma3, IEnumerable<RsiResult> rsi, INotifier notifier, IRiskManagement riskManagement, ILogger logger, ITime time)
    {
        _candleRepository = candleRepository;
        _indicatorsOptions = indicatorsOptions;
        _smma1 = smma1;
        _smma2 = smma2;
        _smma3 = smma3;
        _rsi = rsi;
        _notifier = notifier;
        _riskManagement = riskManagement;

        _logger = logger.ForContext<SmmaRsiStrategyProvider>();
        _time = time;
    }

    private void OnLastCandleReached()
    {
        _logger.Information("Raising LastCandleReached event.");
        LastCandleReached?.Invoke(this, EventArgs.Empty);
        _logger.Information("LastCandleReached event raised.");
    }

    private void OnCandleClosed(Candle candle)
    {
        _logger.Information("Raising OnCandleClosed event.");
        CandleClosed?.Invoke(this, new OnCandleCloseEventsArgs(candle));
        _logger.Information("OnCandleClosed event raised.");
    }

    public void BotTicked() => _hasBotTicked = true;

    public void BrokerProcessedCandle() => _hasBrokerProcessedCandle = true;

    public async Task TryMoveToNextCandle()
    {
        int candlesCount = await _candleRepository.CandlesCount();

        if (_index != null && _index < candlesCount - 1 && (!_hasBotTicked || !_hasBrokerProcessedCandle))
        {
            _logger.Information("bot has not ticked yet or broker has not finished processing the candle.");
            return;
        }
        else
        {
            _hasBotTicked = false;
            _hasBrokerProcessedCandle = false;
        }

        if (_index == null)
            await Reset();

        int? oldIndex = _index;
        _index--;

        _logger.Information("index decrease from: {oldIndex} to: {newIndex}", oldIndex, _index);

        if (_smma1.Count() != candlesCount || _smma2.Count() != candlesCount || _smma3.Count() != candlesCount || _rsi.Count() != candlesCount)
            throw new InvalidIndicatorException();

        if (_index < 0)
        {
            _logger.Information("End of candles reached.");
            OnLastCandleReached();
            return;
        }

        bool isUpTrend = IsUpTrend((int)_index!);
        bool isDownTrend = IsDownTrend((int)_index!);
        bool isInTrend = isUpTrend || isDownTrend;

        bool rsiCrossedOverLowerBand = HasRsiCrossedOverLowerBand((int)_index!);
        bool rsiCrossedUnderUpperBand = HasRsiCrossedUnderUpperBand((int)_index!);

        bool shouldOpenPosition = false;
        if (isInTrend)
            if ((isUpTrend && rsiCrossedOverLowerBand) || (isDownTrend && rsiCrossedUnderUpperBand))
                shouldOpenPosition = true;

        if (shouldOpenPosition)
        {
            _logger.Information("Candle is valid for a position, sending the message...");

            IMessage message = await CreateOpenPositionMessage(isUpTrend ? PositionDirection.LONG : PositionDirection.SHORT, (await _candleRepository.GetCandle((int)_index!)).Open, false);
            await _notifier.SendMessage(message);
            _logger.Information("Message sent.");
        }

        Candle candle = await _candleRepository.GetCandle((int)_index!);

        _time.SetUtcNow(candle.Date);

        OnCandleClosed(candle);
    }

    public async Task Reset() => _index = await _candleRepository.CandlesCount();

    public Task GetCandleIndex() => Task.FromResult(_index);

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

    private async Task<IMessage> CreateOpenPositionMessage(string direction, decimal positionEntryPrice, bool hasTPPrice)
    {
        string message = IGeneralMessage.CreateMessageBody(
            openingPosition: true,
            allowingParallelPositions: true,
            closingAllPositions: false,
            direction,
            _riskManagement.GetLeverage(),
            _riskManagement.GetMargin(),
            await _candleRepository.GetTimeFrame(),
            _riskManagement.GetSLPrice(direction, positionEntryPrice),
            hasTPPrice ? _riskManagement.GetTPPrice(direction, positionEntryPrice) : null
        );

        return new Message()
        {
            From = nameof(SmmaRsiStrategyProvider),
            Body = message,
            SentAt = (await _candleRepository.GetCandle((int)_index!)).Date.AddSeconds(await _candleRepository.GetTimeFrame())
        };
    }
}
