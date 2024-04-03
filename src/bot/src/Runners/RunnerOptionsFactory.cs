namespace bot.src.Runners;

public static class RunnerOptionsFactory
{
    public static IRunnerOptions CreateRunnerOptions(string optionsName) => optionsName switch
    {
        RunnerNames.SMMA_RSI => new SmmaRsi.RunnerOptions(),
        RunnerNames.EMA_RSI => new EmaRsi.RunnerOptions(),
        RunnerNames.STOCHASTIC_EMA => new StochasticEma.RunnerOptions(),
        RunnerNames.SUPER_TREND_V1 => new SuperTrendV1.RunnerOptions(),
        RunnerNames.UT_BOT => new UtBot.RunnerOptions(),
        RunnerNames.DOUBLE_UT_BOT => new DoubleUtBot.RunnerOptions(),
        RunnerNames.LUCK => new Luck.RunnerOptions(),
        RunnerNames.CANDLES_OPEN_CLOSE => new CandlesOpenClose.RunnerOptions(),
        _ => throw new Exception("Invalid runner option name provided.")
    };
}
