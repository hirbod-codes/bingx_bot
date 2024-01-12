using bot.src.MessageStores;
using bot.src.MessageStores.Models;
using Serilog;

namespace bot.src.Strategies.GeneralStrategy;

public class GeneralStrategy : IStrategy
{
    public const string MESSAGE_DELIMITER = "%%%68411864%%%";
    public const string FIELD_DELIMITER = ",";
    public const string KEY_VALUE_PAIR_DELIMITER = "=";
    private bool _allowingParallelPositions;
    private bool _closingAllPositions;
    private bool _direction;
    private float _leverage;
    private float _margin;
    private readonly IMessageStore _messageStore;
    private readonly ILogger _logger;
    private bool _openingPosition;
    private string? _previousMessageId = null;
    private readonly string _provider;
    private float _slPrice;
    private int _timeFrame;
    private float? _tpPrice = null;

    public GeneralStrategy(string provider, IMessageStore messageStore, ILogger logger)
    {
        _provider = provider;
        _messageStore = messageStore;
        _logger = logger.ForContext<GeneralStrategy>();
    }

    public async Task<bool> CheckForSignal()
    {
        _logger.Information("Checking for signals.");

        Message? message = await _messageStore.GetLastMessage(from: _provider);

        if (message is null)
        {
            _logger.Information("No message found.");
            return false;
        }

        if (message.From != _provider)
        {
            _logger.Information("Received a message with invalid provider.");
            throw new InvalidProviderException();
        }

        if (!message.Body.Contains(MESSAGE_DELIMITER))
        {
            _logger.Information("Message has no signal!");
            return false;
        }

        if (message.Id == _previousMessageId)
        {
            _logger.Information("This message is already processed.");
            return false;
        }
        else
            _previousMessageId = message.Id;

        ParseMessage(message);

        if ((_openingPosition && _closingAllPositions) || (!_openingPosition && !_closingAllPositions))
        {
            InvalidSignalException ex = new();
            _logger.Error(ex, "This message is already processed.");
            throw ex;
        }

        if (message.SentAt.AddSeconds(_timeFrame) < DateTime.UtcNow)
        {
            ExpiredSignalException ex = new();
            _logger.Error(ex, "This message is expired(too old for this time frame).");
            throw new ExpiredSignalException();
        }

        _logger.Information("Signal received.");
        return true;
    }

    public bool GetDirection() => _direction;

    public float GetLeverage() => _leverage;

    public float GetMargin() => _margin;

    public float GetSLPrice() => _slPrice;

    public int GetTimeFrame() => _timeFrame;

    public float? GetTPPrice() => _tpPrice;

    public bool IsParallelPositionsAllowed() => _allowingParallelPositions;

    public bool ShouldCloseAllPositions() => _closingAllPositions;

    public bool ShouldOpenPosition() => _openingPosition;

    /// <exception cref="MessageParseException"></exception>
    private void ParseMessage(Message message)
    {
        if (!message.Body.Contains(MESSAGE_DELIMITER) || !message.Body.Contains(FIELD_DELIMITER) || !message.Body.Contains(KEY_VALUE_PAIR_DELIMITER))
        {
            MessageParseException ex = new();
            _logger.Error(ex, "no valid delimiter found.");
            throw ex;
        }

        string fieldsString = message.Body.Split(MESSAGE_DELIMITER, StringSplitOptions.None)[1];
        List<string> strings = fieldsString.Split(FIELD_DELIMITER, StringSplitOptions.TrimEntries).ToList();
        Dictionary<string, string> fields = strings.ConvertAll(s =>
        {
            string[] kv = s.Split(KEY_VALUE_PAIR_DELIMITER, StringSplitOptions.TrimEntries);

            return KeyValuePair.Create(kv[0], kv[1]);
        }).ToDictionary(keySelector: o => o.Key, elementSelector: o => o.Value);

        // Parse
        if (fields.TryGetValue(nameof(_allowingParallelPositions), out string? allowingParallelPositionsString) && TryParseBoolean(allowingParallelPositionsString, out bool allowingParallelPositions))
            _allowingParallelPositions = allowingParallelPositions;
        else
        {
            MessageParseException ex = new();
            _logger.Error(ex, $"Invalid value provided for {nameof(_allowingParallelPositions)} property");
            throw ex;
        }

        if (fields.TryGetValue(nameof(_closingAllPositions), out string? closingAllPositionsString) && TryParseBoolean(closingAllPositionsString, out bool closingAllPositions))
            _closingAllPositions = closingAllPositions;
        else
        {
            MessageParseException ex = new();
            _logger.Error(ex, $"Invalid value provided for {nameof(_closingAllPositions)} property");
            throw ex;
        }

        if (fields.TryGetValue(nameof(_direction), out string? directionString) && TryParseBoolean(directionString, out bool direction))
            _direction = direction;
        else
        {
            MessageParseException ex = new();
            _logger.Error(ex, $"Invalid value provided for {nameof(_direction)} property");
            throw ex;
        }

        if (fields.TryGetValue(nameof(_leverage), out string? leverageString) && float.TryParse(leverageString, out float leverage) && leverage > 0)
            _leverage = leverage;
        else
        {
            MessageParseException ex = new();
            _logger.Error(ex, $"Invalid value provided for {nameof(_leverage)} property");
            throw ex;
        }

        if (fields.TryGetValue(nameof(_margin), out string? marginString) && float.TryParse(marginString, out float margin) && margin > 0)
            _margin = margin;
        else
        {
            MessageParseException ex = new();
            _logger.Error(ex, $"Invalid value provided for {nameof(_margin)} property");
            throw ex;
        }

        if (fields.TryGetValue(nameof(_openingPosition), out string? openingPositionString) && TryParseBoolean(openingPositionString, out bool openingPosition))
            _openingPosition = openingPosition;
        else
        {
            MessageParseException ex = new();
            _logger.Error(ex, $"Invalid value provided for {nameof(_openingPosition)} property");
            throw ex;
        }

        if (fields.TryGetValue(nameof(_slPrice), out string? slPriceString) && float.TryParse(slPriceString, out float slPrice) && slPrice > 0)
            _slPrice = slPrice;
        else
        {
            MessageParseException ex = new();
            _logger.Error(ex, $"Invalid value provided for {nameof(_slPrice)} property");
            throw ex;
        }

        if (fields.TryGetValue(nameof(_timeFrame), out string? timeFrameString) && int.TryParse(timeFrameString, out int timeFrame) && timeFrame > 0)
            _timeFrame = timeFrame;
        else
        {
            MessageParseException ex = new();
            _logger.Error(ex, $"Invalid value provided for {nameof(_timeFrame)} property");
            throw ex;
        }

        if (fields.TryGetValue(nameof(_tpPrice), out string? tpPriceString) && tpPriceString is not null && float.TryParse(tpPriceString, out float tpPrice) && tpPrice > 0)
            _tpPrice = tpPrice;
        else if (tpPriceString is not null)
        {
            MessageParseException ex = new();
            _logger.Error(ex, $"Invalid value provided for {nameof(_tpPrice)} property");
            throw ex;
        }
    }

    private static bool TryParseBoolean(string allowingParallelPositionsString, out bool allowingParallelPositions) =>
        bool.TryParse(allowingParallelPositionsString.Contains('0') ? "false" : (allowingParallelPositionsString.Contains('1') ? "true" : allowingParallelPositionsString), out allowingParallelPositions);
}
