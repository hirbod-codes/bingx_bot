using SmmaRsiRiskManagementOptions = bot.src.RiskManagement.SmmaRsi.RiskManagementOptions;
using DoubleUtBotRiskManagementOptions = bot.src.RiskManagement.DoubleUtBot.RiskManagementOptions;
using UtBotRiskManagementOptions = bot.src.RiskManagement.UtBot.RiskManagementOptions;

namespace bot.src.RiskManagement;

public static class RiskManagementOptionsFactory
{
    public static IRiskManagementOptions RiskManagementOptions(string riskManagementName) => riskManagementName switch
    {
        RiskManagementNames.SMMA_RSI => new SmmaRsiRiskManagementOptions(),
        RiskManagementNames.UT_BOT => new UtBotRiskManagementOptions(),
        RiskManagementNames.DOUBLE_UT_BOT => new DoubleUtBotRiskManagementOptions(),
        _ => throw new Exception()
    };
}
