using bot.src.Bots.General;
using bot.src.Brokers;
using bot.src.MessageStores;
using bot.src.RiskManagement;
using bot.src.Util;
using Serilog;

namespace bot.src.Bots;

public static class BotFactory
{
    public static IBot CreateBot(string botName, IBroker broker, IBotOptions botOptions, IMessageStore messageStore, IRiskManagement riskManagement, ITime time, ILogger logger) => botName switch
    {
        "General" => new Bot(botOptions, broker, time, messageStore, riskManagement, logger),
        _ => throw new Exception()
    };
}
