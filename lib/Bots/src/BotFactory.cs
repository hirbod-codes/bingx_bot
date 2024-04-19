using ILogger = Serilog.ILogger;
using Abstractions.src.Bots;
using Abstractions.src.Brokers;
using Abstractions.src.MessageStores;
using Abstractions.src.RiskManagement;
using Abstractions.src.Utilities;
using Abstractions.src.Notifiers;

namespace Bots.src;

public static class BotFactory
{
    public static IBot CreateBot(string botName, IBroker broker, IBotOptions botOptions, IMessageStore messageStore, IRiskManagement riskManagement, ITime time, INotifier notifier, ILogger logger) => botName switch
    {
        BotNames.SUPER_TREND_V1 => new SuperTrendV1.Bot(botOptions, broker, time, messageStore, riskManagement, logger, notifier),
        _ => throw new Exception()
    };
}
