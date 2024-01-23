using bot.src.Brokers;
using bot.src.Data;
using bot.src.PnLAnalysis;
using bot.src.Util;
using SmmaRsiRiskManagement = bot.src.RiskManagement.SmmaRsi.RiskManagement;

namespace bot.src.RiskManagement;

public static class RiskManagementFactory
{
    public static IRiskManagement CreateRiskManager(string riskManagementName, IRiskManagementOptions riskManagementOptions, IBroker broker, ITime time) => riskManagementName switch
    {
        RiskManagementNames.SMMA_RSI => new SmmaRsiRiskManagement(riskManagementOptions, broker, time),
        _ => throw new Exception()
    };
}
