using bot.src.Brokers;
using bot.src.Util;
using Serilog;

namespace bot.src.RiskManagement;

public static class RiskManagementFactory
{
    public static IRiskManagement CreateRiskManager(string riskManagementName, IRiskManagementOptions riskManagementOptions, IBroker broker, ITime time, ILogger logger) => riskManagementName switch
    {
        RiskManagementNames.UT_BOT => new UtBot.RiskManagement(riskManagementOptions, broker, time),
        RiskManagementNames.DOUBLE_UT_BOT => new DoubleUtBot.RiskManagement(riskManagementOptions, broker, time),
        RiskManagementNames.SMMA_RSI => new SmmaRsi.RiskManagement(riskManagementOptions, broker, time),
        RiskManagementNames.EMA_RSI => new EmaRsi.RiskManagement(riskManagementOptions, broker, time),
        RiskManagementNames.STOCHASTIC_EMA => new StochasticEma.RiskManagement(riskManagementOptions, broker, time),
        RiskManagementNames.LUCK => new Luck.RiskManagement(riskManagementOptions, broker, time, logger),
        RiskManagementNames.CANDLES_OPEN_CLOSE => new CandlesOpenClose.RiskManagement(riskManagementOptions, broker, time, logger),
        _ => throw new Exception()
    };
}
