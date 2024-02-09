using GeneralBot = bot.src.Bots.General.Bot;
using UtBotBot = bot.src.Bots.UtBot.Bot;
using bot.src.Brokers;
using bot.src.MessageStores;
using bot.src.RiskManagement;
using bot.src.Util;
using Serilog;
using bot.src.Notifiers;

namespace bot.src.Bots;

public static class BotFactory
{
    public static IBot CreateBot(string botName, IBroker broker, IBotOptions botOptions, IMessageStore messageStore, IRiskManagement riskManagement, ITime time, INotifier notifier, ILogger logger) => botName switch
    {
        BotNames.GENERAL => new GeneralBot(botOptions, broker, time, messageStore, riskManagement, logger),
        BotNames.UT_BOT => new UtBotBot(botOptions, broker, time, messageStore, riskManagement, logger, notifier),
        _ => throw new Exception()
    };
}
