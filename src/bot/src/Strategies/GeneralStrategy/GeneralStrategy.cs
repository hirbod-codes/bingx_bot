using bot.src.MessageStores;
using bot.src.Util;
using Serilog;

namespace bot.src.Strategies.GeneralStrategy;

public class GeneralStrategy : IStrategy
{
    private bool _allowingParallelPositions;
    private bool _closingAllPositions;
    private string _direction = null!;
    private decimal _leverage;
    private decimal _margin;
    private readonly IMessageStore _messageStore;
    private readonly ILogger _logger;
    private readonly ITime _time;
    private bool _openingPosition;
    private string? _previousMessageId = null;
    private readonly string _provider;
    private decimal _slPrice;
    private int _timeFrame;
    private decimal? _tpPrice = null;

    public GeneralStrategy(string provider, IMessageStore messageStore, ILogger logger, ITime time)
    {
        _provider = provider;
        _messageStore = messageStore;
        _logger = logger.ForContext<GeneralStrategy>();
        _time = time;
    }

    public async Task<bool> CheckForSignal()
    {
        _logger.Information("Checking for signals.");

        IMessage? rawMessage = await _messageStore.GetLastMessage(from: _provider);

        if (rawMessage is null)
        {
            _logger.Information("No message found.");
            return false;
        }

        IGeneralMessage? message = IGeneralMessage.CreateMessage(new GeneralMessage(), rawMessage);

        if (message is null)
        {
            _logger.Information("Message has no signal!");
            return false;
        }

        if (message.From != _provider)
        {
            _logger.Information("Received a message with invalid provider.");
            throw new InvalidProviderException();
        }

        if (message.Id == _previousMessageId)
        {
            _logger.Information("This message is already processed.");
            return false;
        }
        else
            _previousMessageId = message.Id;

        CollectMessageInformation(message);

        if ((_openingPosition && _closingAllPositions) || (!_openingPosition && !_closingAllPositions))
        {
            InvalidSignalException ex = new();
            _logger.Error(ex, "This message is already processed.");
            throw ex;
        }

        if (message.SentAt.AddSeconds(_timeFrame) < _time.GetUtcNow())
        {
            ExpiredSignalException ex = new();
            _logger.Error(ex, "This message is expired(too old for this time frame).");
            throw new ExpiredSignalException();
        }

        _logger.Information("Signal received.");
        return true;
    }

    public string GetDirection() => _direction;

    public decimal GetLeverage() => _leverage;

    public decimal GetMargin() => _margin;

    public decimal GetSLPrice() => _slPrice;

    public int GetTimeFrame() => _timeFrame;

    public decimal? GetTPPrice() => _tpPrice;

    public bool IsParallelPositionsAllowed() => _allowingParallelPositions;

    public bool ShouldCloseAllPositions() => _closingAllPositions;

    public bool ShouldOpenPosition() => _openingPosition;

    private void CollectMessageInformation(IGeneralMessage message)
    {
        _allowingParallelPositions = message.AllowingParallelPositions;
        _closingAllPositions = message.ClosingAllPositions;
        _direction = message.Direction;
        _leverage = message.Leverage;
        _margin = message.Margin;
        _openingPosition = message.OpeningPosition;
        _slPrice = message.SlPrice;
        _timeFrame = message.TimeFrame;
        _tpPrice = message.TpPrice;

        return;
    }
}
