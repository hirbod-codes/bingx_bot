using bot.src.Brokers;
using bot.src.Util;
using SmmaRsiRiskManagement = bot.src.RiskManagement.SmmaRsi.RiskManagement;
using EmaRsiRiskManagement = bot.src.RiskManagement.EmaRsi.RiskManagement;
using DoubleUtBotRiskManagement = bot.src.RiskManagement.DoubleUtBot.RiskManagement;
using UtBotRiskManagement = bot.src.RiskManagement.UtBot.RiskManagement;

namespace bot.src.RiskManagement;

public static class RiskManagementFactory
{
    public static IRiskManagement CreateRiskManager(string riskManagementName, IRiskManagementOptions riskManagementOptions, IBroker broker, ITime time) => riskManagementName switch
    {
        RiskManagementNames.UT_BOT => new UtBotRiskManagement(riskManagementOptions),
        RiskManagementNames.DOUBLE_UT_BOT => new DoubleUtBotRiskManagement(riskManagementOptions),
        RiskManagementNames.SMMA_RSI => new SmmaRsiRiskManagement(riskManagementOptions, broker, time),
        RiskManagementNames.EMA_RSI => new EmaRsiRiskManagement(riskManagementOptions, broker, time),
        _ => throw new Exception()
    };
}
