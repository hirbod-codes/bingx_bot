using BotITime = bot.src.Util.ITime;

namespace providers.src;

public interface ITime : BotITime
{
    public void SetUtcNow(DateTime dateTime);
}

