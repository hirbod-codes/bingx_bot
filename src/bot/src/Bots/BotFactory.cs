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
        BotNames.GENERAL => new General.Bot(botOptions, broker, time, messageStore, riskManagement, logger),
        BotNames.SUPER_TREND_V1 => new SuperTrendV1.Bot(botOptions, broker, time, messageStore, riskManagement, logger, notifier),
        BotNames.UT_BOT => new UtBot.Bot(botOptions, broker, time, messageStore, riskManagement, logger, notifier),
        BotNames.DOUBLE_UT_BOT => new DoubleUtBot.Bot(botOptions, broker, time, messageStore, riskManagement, logger, notifier),
        BotNames.CANDLES_OPEN_CLOSE => new CandlesOpenClose.Bot(botOptions, broker, time, messageStore, riskManagement, logger, notifier),
        _ => throw new Exception()
    };
}
