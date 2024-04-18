using Abstractions.src.Bots;
using Abstractions.src.Brokers;
using Abstractions.src.MessageStore;
using Abstractions.src.Notifiers;
using Abstractions.src.RiskManagements;
using Abstractions.src.Utilities;
using ILogger = Serilog.ILogger;

namespace Bots.src;

public static class BotFactory
{
    public static IBot CreateBot(string botName, IBroker broker, IBotOptions botOptions, IMessageStore messageStore, IRiskManagement riskManagement, ITime time, INotifier notifier, ILogger logger) => botName switch
    {
        BotNames.SUPER_TREND_V1 => new SuperTrendV1.Bot(botOptions, broker, time, messageStore, riskManagement, logger, notifier),
        _ => throw new Exception()
    };
}
