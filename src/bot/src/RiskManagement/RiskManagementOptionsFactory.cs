namespace bot.src.RiskManagement;

public static class RiskManagementOptionsFactory
{
    public static IRiskManagementOptions RiskManagementOptions(string riskManagementName) => riskManagementName switch
    {
        RiskManagementNames.SMMA_RSI => new SmmaRsi.RiskManagementOptions(),
        RiskManagementNames.EMA_RSI => new EmaRsi.RiskManagementOptions(),
        RiskManagementNames.UT_BOT => new UtBot.RiskManagementOptions(),
        RiskManagementNames.DOUBLE_UT_BOT => new DoubleUtBot.RiskManagementOptions(),
        RiskManagementNames.STOCHASTIC_EMA => new StochasticEma.RiskManagementOptions(),
        RiskManagementNames.EMA_STOCHASTIC_SUPER_TREND => new EmaStochasticSuperTrend.RiskManagementOptions(),
        RiskManagementNames.LUCK => new Luck.RiskManagementOptions(),
        RiskManagementNames.CANDLES_OPEN_CLOSE => new CandlesOpenClose.RiskManagementOptions(),
        _ => throw new Exception()
    };
}
