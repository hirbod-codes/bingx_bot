using bot.src.Data.Models;
using bot.src.MessageStores;

namespace bot.src.Bots.CandlesOpenClose.Models;

public class Message : IMessage
{
    public const string MESSAGE_DELIMITER = "%%%68411864%%%";
    public const string FIELD_DELIMITER = ",";
    public const string KEY_VALUE_PAIR_DELIMITER = "=";
    public string Id { get; set; } = null!;
    public string From { get; set; } = null!;
    public string Body { get; set; } = null!;
    public DateTime SentAt { get; set; }
    public bool AllowingParallelPositions { get; set; }
    public bool ClosingAllPositions { get; set; }
    public string Direction { get; set; } = null!;
    public bool OpeningPosition { get; set; }
    public decimal Limit { get; set; }
    public decimal? TpPrice { get; set; }
    public decimal SlPrice { get; set; }

    public static string CreateMessageBody(bool openingPosition, bool allowingParallelPositions, bool closingAllPositions, string direction, decimal limit, decimal slPrice, decimal? tpPrice = null)
    {
        string message = $"{MESSAGE_DELIMITER}";

        message += $"{nameof(AllowingParallelPositions)}{KEY_VALUE_PAIR_DELIMITER}{(allowingParallelPositions ? "1" : "0")}";
        message += $"{FIELD_DELIMITER}";

        message += $"{nameof(ClosingAllPositions)}{KEY_VALUE_PAIR_DELIMITER}{(closingAllPositions ? "1" : "0")}";
        message += $"{FIELD_DELIMITER}";

        message += $"{nameof(Direction)}{KEY_VALUE_PAIR_DELIMITER}{direction}";
        message += $"{FIELD_DELIMITER}";

        message += $"{nameof(OpeningPosition)}{KEY_VALUE_PAIR_DELIMITER}{(openingPosition ? "1" : "0")}";
        message += $"{FIELD_DELIMITER}";

        message += $"{nameof(Limit)}{KEY_VALUE_PAIR_DELIMITER}{limit}";
        message += $"{FIELD_DELIMITER}";

        message += $"{nameof(SlPrice)}{KEY_VALUE_PAIR_DELIMITER}{slPrice}";

        if (tpPrice != null)
        {
            message += $"{FIELD_DELIMITER}";
            message += $"{nameof(TpPrice)}{KEY_VALUE_PAIR_DELIMITER}{tpPrice}";
        }

        message += $"{MESSAGE_DELIMITER}";

        return message;
    }

    /// <exception cref="MessageParseException"></exception>
    public static Message? CreateMessage(Message generalMessage, IMessage message)
    {
        if (!message.Body.Contains(MESSAGE_DELIMITER))
            return null;

        if (!message.Body.Contains(FIELD_DELIMITER) || !message.Body.Contains(KEY_VALUE_PAIR_DELIMITER))
            throw new MessageParseException();

        generalMessage.Id = message.Id;
        generalMessage.From = message.From;
        generalMessage.SentAt = message.SentAt;

        string fieldsString = message.Body.Split(MESSAGE_DELIMITER, StringSplitOptions.None)[1];
        List<string> strings = fieldsString.Split(FIELD_DELIMITER, StringSplitOptions.RemoveEmptyEntries).ToList();
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

        if (fields.TryGetValue(nameof(Direction), out string? direction) && (direction == PositionDirection.LONG || direction == PositionDirection.SHORT))
            generalMessage.Direction = direction;
        else
            throw new MessageParseException();

        if (fields.TryGetValue(nameof(OpeningPosition), out string? openingPositionString) && TryParseBoolean(openingPositionString, out bool openingPosition))
            generalMessage.OpeningPosition = openingPosition;
        else
            throw new MessageParseException();

        if (fields.TryGetValue(nameof(Limit), out string? limitString) && decimal.TryParse(limitString, out decimal limit))
            generalMessage.Limit = limit;
        else
            throw new MessageParseException();

        if (fields.TryGetValue(nameof(SlPrice), out string? slPriceString) && decimal.TryParse(slPriceString, out decimal slPrice))
            generalMessage.SlPrice = slPrice;
        else
            throw new MessageParseException();

        if (fields.TryGetValue(nameof(TpPrice), out string? tpPriceString) && tpPriceString is not null && decimal.TryParse(tpPriceString, out decimal tpPrice))
            generalMessage.TpPrice = tpPrice;
        else if (tpPriceString is not null)
            throw new MessageParseException();

        return generalMessage;
    }

    private static bool TryParseBoolean(string allowingParallelPositionsString, out bool allowingParallelPositions) =>
        bool.TryParse(allowingParallelPositionsString.Contains('0') ? "false" : (allowingParallelPositionsString.Contains('1') ? "true" : allowingParallelPositionsString), out allowingParallelPositions);
}
