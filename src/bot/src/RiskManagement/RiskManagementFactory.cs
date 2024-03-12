using bot.src.Brokers;
using bot.src.Util;

namespace bot.src.RiskManagement;

public static class RiskManagementFactory
{
    public static IRiskManagement CreateRiskManager(string riskManagementName, IRiskManagementOptions riskManagementOptions, IBroker broker, ITime time) => riskManagementName switch
    {
        RiskManagementNames.UT_BOT => new UtBot.RiskManagement(riskManagementOptions),
        RiskManagementNames.DOUBLE_UT_BOT => new DoubleUtBot.RiskManagement(riskManagementOptions),
        RiskManagementNames.SMMA_RSI => new SmmaRsi.RiskManagement(riskManagementOptions, broker, time),
        RiskManagementNames.EMA_RSI => new EmaRsi.RiskManagement(riskManagementOptions, broker, time),
        RiskManagementNames.STOCHASTIC_EMA => new StochasticEma.RiskManagement(riskManagementOptions, broker, time),
        _ => throw new Exception()
    };
}
