using bot.src.Bots.General;
using bot.src.Brokers;
using bot.src.MessageStores;
using bot.src.Util;
using Serilog;

namespace bot.src.Bots;

public static class BotFactory
{
    public static IBot CreateBot(string botName, IBroker broker, IBotOptions botOptions, IMessageStore messageStore, ITime time, ILogger logger) => botName switch
    {
        "General" => new Bot(botOptions, broker, time, logger, messageStore),
        _ => throw new Exception()
    };
}
