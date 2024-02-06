using GeneralBotOptions = bot.src.Bots.General.BotOptions;
using UtBotBotOptions = bot.src.Bots.UtBot.BotOptions;

namespace bot.src.Bots;

public static class BotOptionsFactory
{
    public static IBotOptions CreateBotOptions(string botName) => botName switch
    {
        BotNames.GENERAL => new GeneralBotOptions(),
        BotNames.UT_BOT => new UtBotBotOptions(),
        _ => throw new Exception("Invalid bot options name.")
    };
}
