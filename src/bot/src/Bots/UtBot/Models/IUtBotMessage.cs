using bot.src.Data.Models;
using bot.src.MessageStores;

namespace bot.src.Bots.UtBot.Models;

public interface IUtBotMessage : IMessage
{
    public const string MESSAGE_DELIMITER = "%%%68411864%%%";
    public const string FIELD_DELIMITER = ",";
    public const string KEY_VALUE_PAIR_DELIMITER = "=";
    public string Direction { get; set; }
    public bool OpeningPosition { get; set; }
    public decimal SlPrice { get; set; }

    public static string CreateMessageBody(bool openingPosition, string direction, decimal slPrice, decimal? tpPrice = null)
    {
        string message = $"{MESSAGE_DELIMITER}";

        message += $"{nameof(Direction)}{KEY_VALUE_PAIR_DELIMITER}{direction}";
        message += $"{FIELD_DELIMITER}";

        message += $"{nameof(OpeningPosition)}{KEY_VALUE_PAIR_DELIMITER}{(openingPosition ? "1" : "0")}";
        message += $"{FIELD_DELIMITER}";

        message += $"{nameof(SlPrice)}{KEY_VALUE_PAIR_DELIMITER}{slPrice}";

        message += $"{MESSAGE_DELIMITER}";

        return message;
    }

    /// <exception cref="MessageParseException"></exception>
    public static IUtBotMessage? CreateMessage(IUtBotMessage generalMessage, IMessage message)
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
        if (fields.TryGetValue(nameof(Direction), out string? direction) && (direction == PositionDirection.LONG || direction == PositionDirection.SHORT || direction == "0" || direction == "1"))
            generalMessage.Direction = direction == "1" ? PositionDirection.LONG : (direction == "0" ? PositionDirection.SHORT : direction);
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

        return generalMessage;
    }

    private static bool TryParseBoolean(string allowingParallelPositionsString, out bool allowingParallelPositions) =>
        bool.TryParse(allowingParallelPositionsString.Contains('0') ? "false" : (allowingParallelPositionsString.Contains('1') ? "true" : allowingParallelPositionsString), out allowingParallelPositions);
}
