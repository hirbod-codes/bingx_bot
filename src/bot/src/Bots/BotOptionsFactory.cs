using bot.src.Bots.General;

namespace bot.src.Bots;

public static class BotOptionsFactory
{
    public static IBotOptions CreateBotOptions(string botName) => botName switch
    {
        BotNames.GENERAL => new BotOptions(),
        _ => throw new Exception()
    };
}
