using Abstractions.src.MessageStore;
using Abstractions.src.Models;

namespace Strategies.src.SuperTrendV1.Models;

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
    public decimal EntryPrice { get; set; }

    public static string CreateMessageBody(bool openingPosition, bool allowingParallelPositions, bool closingAllPositions, string direction, decimal entryPrice)
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

        message += $"{nameof(EntryPrice)}{KEY_VALUE_PAIR_DELIMITER}{entryPrice}";

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

        if (fields.TryGetValue(nameof(EntryPrice), out string? entryPriceString) && decimal.TryParse(entryPriceString, out decimal entryPrice))
            generalMessage.EntryPrice = entryPrice;
        else
            throw new MessageParseException();

        return generalMessage;
    }

    private static bool TryParseBoolean(string allowingParallelPositionsString, out bool allowingParallelPositions) =>
        bool.TryParse(allowingParallelPositionsString.Contains('0') ? "false" : (allowingParallelPositionsString.Contains('1') ? "true" : allowingParallelPositionsString), out allowingParallelPositions);
}
