using bot.src.Data;
using bot.src.Data.Models;
using bot.src.MessageStores;
using bot.src.MessageStores.InMemory.Models;
using bot.src.Notifiers;
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
    private int _index;

    public event EventHandler<OnCandleCloseEventsArgs>? CandleClosed;

    public SmmaRsiStrategyProvider(ICandleRepository candleRepository, IndicatorsOptions indicatorsOptions, IEnumerable<SmmaResult> smma1, IEnumerable<SmmaResult> smma2, IEnumerable<SmmaResult> smma3, IEnumerable<RsiResult> rsi, INotifier notifier, IRiskManagement riskManagement, ILogger logger)
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
    }

    public void OnCandleClosed(Candle candle)
    {
        _logger.Information("candle closed with index: {index}", _index);
        CandleClosed?.Invoke(this, new OnCandleCloseEventsArgs(candle));
    }

    public async Task MoveToNextCandle()
    {
        _index++;

        if (_index >= await _candleRepository.CandlesCount())
            return;

        OnCandleClosed(await _candleRepository.GetCandle(_index));

        bool isUpTrend = IsUpTrend(_index);
        bool isDownTrend = IsDownTrend(_index);
        bool isInTrend = isUpTrend || isDownTrend;

        bool rsiCrossedOverLowerBand = HasRsiCrossedOverLowerBand(_index);
        bool rsiCrossedUnderUpperBand = HasRsiCrossedUnderUpperBand(_index);

        bool shouldOpenPosition = false;
        if (isInTrend)
            if ((isUpTrend && rsiCrossedOverLowerBand) || (isDownTrend && rsiCrossedUnderUpperBand))
                shouldOpenPosition = true;

        if (shouldOpenPosition)
        {
            IMessage message = await CreateOpenPositionMessageAsync(isUpTrend ? PositionDirection.LONG : PositionDirection.SHORT, (await _candleRepository.GetCandle(_index)).Open, false);
            await _notifier.SendMessage(message);
        }

        return;
    }

    public Task Reset()
    {
        _index = -1;
        return Task.CompletedTask;
    }

    public Task GetCandleIndex() => Task.FromResult(_index);

    private bool HasRsiCrossedUnderUpperBand(int index)
    {
        if (_rsi.ElementAtOrDefault(index) is null || _rsi.ElementAtOrDefault(index - 1) is null)
            return false;

        return _rsi.ElementAt(index).Rsi > _indicatorsOptions.Rsi.UpperBand && _rsi.ElementAt(index - 1).Rsi < _indicatorsOptions.Rsi.LowerBand;
    }

    private bool HasRsiCrossedOverLowerBand(int index)
    {
        if (_rsi.ElementAtOrDefault(index) is null || _rsi.ElementAtOrDefault(index - 1) is null)
            return false;

        return _rsi.ElementAt(index).Rsi < _indicatorsOptions.Rsi.LowerBand && _rsi.ElementAt(index - 1).Rsi > _indicatorsOptions.Rsi.UpperBand;
    }

    private bool IsDownTrend(int index) => _smma1.ElementAt(index).Smma < _smma2.ElementAt(index).Smma && _smma2.ElementAt(index).Smma < _smma3.ElementAt(index).Smma;

    private bool IsUpTrend(int index) => _smma1.ElementAt(index).Smma > _smma2.ElementAt(index).Smma && _smma2.ElementAt(index).Smma > _smma3.ElementAt(index).Smma;

    private async Task<IMessage> CreateOpenPositionMessageAsync(string direction, decimal positionEntryPrice, bool hasTPPrice)
    {
        string message = $"{IGeneralMessage.MESSAGE_DELIMITER}";

        message += $"{nameof(IGeneralMessage.AllowingParallelPositions)}{IGeneralMessage.KEY_VALUE_PAIR_DELIMITER}1";
        message += $"{IGeneralMessage.FIELD_DELIMITER}";

        message += $"{nameof(IGeneralMessage.ClosingAllPositions)}{IGeneralMessage.KEY_VALUE_PAIR_DELIMITER}0";
        message += $"{IGeneralMessage.FIELD_DELIMITER}";

        message += $"{nameof(IGeneralMessage.Direction)}{IGeneralMessage.KEY_VALUE_PAIR_DELIMITER}{(direction == PositionDirection.LONG ? "1" : "0")}";
        message += $"{IGeneralMessage.FIELD_DELIMITER}";

        message += $"{nameof(IGeneralMessage.OpeningPosition)}{IGeneralMessage.KEY_VALUE_PAIR_DELIMITER}1";
        message += $"{IGeneralMessage.FIELD_DELIMITER}";

        message += $"{nameof(IGeneralMessage.Leverage)}{IGeneralMessage.KEY_VALUE_PAIR_DELIMITER}{_riskManagement.GetLeverage()}";
        message += $"{IGeneralMessage.FIELD_DELIMITER}";

        message += $"{nameof(IGeneralMessage.Margin)}{IGeneralMessage.KEY_VALUE_PAIR_DELIMITER}{_riskManagement.GetMargin()}";
        message += $"{IGeneralMessage.FIELD_DELIMITER}";

        message += $"{nameof(IGeneralMessage.SlPrice)}{IGeneralMessage.KEY_VALUE_PAIR_DELIMITER}{_riskManagement.GetSLPrice(direction, positionEntryPrice)}";
        message += $"{IGeneralMessage.FIELD_DELIMITER}";

        message += $"{nameof(IGeneralMessage.TimeFrame)}{IGeneralMessage.KEY_VALUE_PAIR_DELIMITER}{await _candleRepository.GetTimeFrame()}";
        message += $"{IGeneralMessage.FIELD_DELIMITER}";

        if (hasTPPrice)
            message += $"{nameof(IGeneralMessage.TpPrice)}{IGeneralMessage.KEY_VALUE_PAIR_DELIMITER}{_riskManagement.GetTPPrice(direction, positionEntryPrice)}";

        message += $"{IGeneralMessage.MESSAGE_DELIMITER}";

        return new Message()
        {
            From = nameof(SmmaRsiStrategyProvider),
            Body = message,
            SentAt = (await _candleRepository.GetCandle(_index)).Date.AddSeconds(await _candleRepository.GetTimeFrame())
        };
    }
}

public class IndicatorsOptions
{
    public SmmaOptions Smma1 { get; set; } = null!;
    public SmmaOptions Smma2 { get; set; } = null!;
    public SmmaOptions Smma3 { get; set; } = null!;
    public RsiOptions Rsi { get; set; } = null!;
}

public class RsiOptions
{
    public int Period { get; set; }
    public string Source { get; set; } = null!;
    public int UpperBand { get; set; }
    public int LowerBand { get; set; }
}

public class SmmaOptions
{
    public int Period { get; set; }
    public string Source { get; set; } = null!;
}
