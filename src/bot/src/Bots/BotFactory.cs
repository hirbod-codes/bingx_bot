using bot.src.Brokers;
using bot.src.Strategies;
using bot.src.Util;
using Serilog;

namespace bot.src.Bots;

public static class BotFactory
{
    public static IBot CreateBot(string botName, IBroker broker, IStrategy strategy, ITime time, ILogger logger) => botName switch
    {
        "General" => new GeneralBot(strategy, broker, time, logger),
        _ => throw new Exception()
    };
}
