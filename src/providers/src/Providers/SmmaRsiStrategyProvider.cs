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
    private int _candlesCount;

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

    public async Task Initiate()
    {
        if (_index == null)
            await Reset();

        _candlesCount = await _candleRepository.CandlesCount();

        if (_smma1.Count() != _candlesCount || _smma2.Count() != _candlesCount || _smma3.Count() != _candlesCount || _rsi.Count() != _candlesCount)
            throw new InvalidIndicatorException();
    }

    public async Task Reset() => _index = await _candleRepository.CandlesCount();

    public async Task<Candle> GetClosedCandle() => await _candleRepository.GetCandle((int)_index!);

    public async Task<bool> TryMoveToNextCandle()
    {
        int? oldIndex = _index;
        _index--;
        _logger.Information("index decrease from: {oldIndex} to: {newIndex}", oldIndex, _index);

        if (_index < 0)
        {
            _logger.Information("End of candles reached.");
            return false;
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

            IMessage message = await CreateOpenPositionMessage(isUpTrend ? PositionDirection.LONG : PositionDirection.SHORT, hasTPPrice: true);
            await _notifier.SendMessage(message);
            _logger.Information("Message sent.");
        }

        Candle candle = await GetClosedCandle();

        _time.SetUtcNow(candle.Date);

        return true;
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

    private async Task<IMessage> CreateOpenPositionMessage(string direction, bool hasTPPrice)
    {
        string message = IGeneralMessage.CreateMessageBody(
            openingPosition: true,
            allowingParallelPositions: true,
            closingAllPositions: false,
            direction,
            _riskManagement.GetLeverage(),
            _riskManagement.GetMargin(),
            await _candleRepository.GetTimeFrame(),
            _riskManagement.GetSLPrice(direction, (await GetClosedCandle()).Close),
            hasTPPrice ? _riskManagement.GetTPPrice(direction, (await GetClosedCandle()).Close) : null
        );

        return new Message()
        {
            From = nameof(SmmaRsiStrategyProvider),
            Body = message,
            SentAt = (await GetClosedCandle()).Date.AddSeconds(await _candleRepository.GetTimeFrame())
        };
    }
}
