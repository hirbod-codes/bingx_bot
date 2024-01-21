using SmmaRsiRiskManagementOptions = bot.src.RiskManagement.SmmaRsi.RiskManagementOptions;

namespace bot.src.RiskManagement;

public static class RiskManagementOptionsFactory
{
    public static IRiskManagementOptions RiskManagementOptions(string riskManagementName) => riskManagementName switch
    {
        RiskManagementNames.SMMA_RSI => new SmmaRsiRiskManagementOptions(),
        _ => throw new Exception()
    };
}
