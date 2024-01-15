namespace bot.src.MessageStores;

public interface IGeneralMessage : IMessage
{
    public const string MESSAGE_DELIMITER = "%%%68411864%%%";
    public const string FIELD_DELIMITER = ",";
    public const string KEY_VALUE_PAIR_DELIMITER = "=";
    public bool AllowingParallelPositions { get; set; }
    public bool ClosingAllPositions { get; set; }
    public bool Direction { get; set; }
    public decimal Leverage { get; set; }
    public decimal Margin { get; set; }
    public bool OpeningPosition { get; set; }
    public decimal SlPrice { get; set; }
    public int TimeFrame { get; set; }
    public decimal? TpPrice { get; set; }

    /// <exception cref="MessageParseException"></exception>
    public static IGeneralMessage? CreateMessage(IGeneralMessage generalMessage, IMessage message)
    {
        if (!message.Body.Contains(MESSAGE_DELIMITER))
            return null;

        if (!message.Body.Contains(FIELD_DELIMITER) || !message.Body.Contains(KEY_VALUE_PAIR_DELIMITER))
            throw new MessageParseException();

        string fieldsString = message.Body.Split(MESSAGE_DELIMITER, StringSplitOptions.None)[1];
        List<string> strings = fieldsString.Split(FIELD_DELIMITER, StringSplitOptions.TrimEntries).ToList();
        Dictionary<string, string> fields = strings.ConvertAll(s =>
        {
            string[] kv = s.Split(KEY_VALUE_PAIR_DELIMITER, StringSplitOptions.TrimEntries);

            return KeyValuePair.Create(kv[0], kv[1]);
        }).ToDictionary(keySelector: o => o.Key, elementSelector: o => o.Value);

        // Parse
        if (fields.TryGetValue(nameof(AllowingParallelPositions), out string? allowingParallelPositionsString) && TryParseBoolean(allowingParallelPositionsString, out bool allowingParallelPositions))
            generalMessage.AllowingParallelPositions = allowingParallelPositions;
        else
            throw new MessageParseException();

        if (fields.TryGetValue(nameof(ClosingAllPositions), out string? closingAllPositionsString) && TryParseBoolean(closingAllPositionsString, out bool closingAllPositions))
            generalMessage.ClosingAllPositions = closingAllPositions;
        else
            throw new MessageParseException();

        if (fields.TryGetValue(nameof(Direction), out string? directionString) && TryParseBoolean(directionString, out bool direction))
            generalMessage.Direction = direction;
        else
            throw new MessageParseException();

        if (fields.TryGetValue(nameof(Leverage), out string? leverageString) && decimal.TryParse(leverageString, out decimal leverage) && leverage > 0)
            generalMessage.Leverage = leverage;
        else
            throw new MessageParseException();

        if (fields.TryGetValue(nameof(Margin), out string? marginString) && decimal.TryParse(marginString, out decimal margin) && margin > 0)
            generalMessage.Margin = margin;
        else
            throw new MessageParseException();

        if (fields.TryGetValue(nameof(OpeningPosition), out string? openingPositionString) && TryParseBoolean(openingPositionString, out bool openingPosition))
            generalMessage.OpeningPosition = openingPosition;
        else
            throw new MessageParseException();

        if (fields.TryGetValue(nameof(SlPrice), out string? slPriceString) && decimal.TryParse(slPriceString, out decimal slPrice) && slPrice > 0)
            generalMessage.SlPrice = slPrice;
        else
            throw new MessageParseException();

        if (fields.TryGetValue(nameof(TimeFrame), out string? timeFrameString) && int.TryParse(timeFrameString, out int timeFrame) && timeFrame > 0)
            generalMessage.TimeFrame = timeFrame;
        else
            throw new MessageParseException();

        if (fields.TryGetValue(nameof(TpPrice), out string? tpPriceString) && tpPriceString is not null && decimal.TryParse(tpPriceString, out decimal tpPrice) && tpPrice > 0)
            generalMessage.TpPrice = tpPrice;
        else if (tpPriceString is not null)
            throw new MessageParseException();

        return generalMessage;
    }

    private static bool TryParseBoolean(string allowingParallelPositionsString, out bool allowingParallelPositions) =>
        bool.TryParse(allowingParallelPositionsString.Contains('0') ? "false" : (allowingParallelPositionsString.Contains('1') ? "true" : allowingParallelPositionsString), out allowingParallelPositions);
}
