using SmmaRsiRiskManagementOptions = bot.src.RiskManagement.SmmaRsi.RiskManagementOptions;
using UtBotRiskManagementOptions = bot.src.RiskManagement.UtBot.RiskManagementOptions;

namespace bot.src.RiskManagement;

public static class RiskManagementOptionsFactory
{
    public static IRiskManagementOptions RiskManagementOptions(string riskManagementName) => riskManagementName switch
    {
        RiskManagementNames.SMMA_RSI => new SmmaRsiRiskManagementOptions(),
        RiskManagementNames.UT_BOT => new UtBotRiskManagementOptions(),
        _ => throw new Exception()
    };
}
