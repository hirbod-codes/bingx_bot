using SmmaRsiRiskManagement = bot.src.RiskManagement.SmmaRsi.RiskManagement;

namespace bot.src.RiskManagement;

public static class RiskManagementFactory
{
    public static IRiskManagement CreateRiskManager(string riskManagementName, IRiskManagementOptions riskManagementOptions) => riskManagementName switch
    {
        RiskManagementNames.SMMA_RSI => new SmmaRsiRiskManagement(riskManagementOptions),
        _ => throw new Exception()
    };
}
