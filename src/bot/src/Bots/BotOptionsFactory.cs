namespace bot.src.Bots;

public static class BotOptionsFactory
{
    public static IBotOptions CreateBotOptions(string botName) => botName switch
    {
        BotNames.GENERAL => new General.BotOptions(),
        BotNames.DOUBLE_UT_BOT => new DoubleUtBot.BotOptions(),
        BotNames.UT_BOT => new UtBot.BotOptions(),
        BotNames.CANDLES_OPEN_CLOSE => new CandlesOpenClose.BotOptions(),
        _ => throw new Exception("Invalid bot options name.")
    };
}
