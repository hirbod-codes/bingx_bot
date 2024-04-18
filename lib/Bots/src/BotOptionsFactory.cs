using Abstractions.src.Bots;

namespace Bots.src;

public static class BotOptionsFactory
{
    public static IBotOptions CreateBotOptions(string botName) => botName switch
    {
        BotNames.SUPER_TREND_V1 => new SuperTrendV1.BotOptions(),
        _ => throw new Exception("Invalid bot options name.")
    };

    public static Type? GetInstanceType(string? name) => name switch
    {
        null => null,
        BotNames.SUPER_TREND_V1 => typeof(SuperTrendV1.BotOptions),
        _ => throw new Exception()
    };
}
