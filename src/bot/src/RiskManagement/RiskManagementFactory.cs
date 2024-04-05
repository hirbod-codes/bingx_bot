using bot.src.Brokers;
using bot.src.Util;
using ILogger = Serilog.ILogger;

namespace bot.src.RiskManagement;

public static class RiskManagementFactory
{
    public static IRiskManagement CreateRiskManager(string riskManagementName, IRiskManagementOptions riskManagementOptions, IBroker broker, ITime time, ILogger logger) => riskManagementName switch
    {
        RiskManagementNames.SUPER_TREND_V1 => new SuperTrendV1.RiskManagement(riskManagementOptions, broker, time, logger),
        _ => throw new Exception()
    };
}
