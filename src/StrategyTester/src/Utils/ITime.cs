using BotITime = bot.src.Util.ITime;

namespace StrategyTester.src.Utils;

public interface ITime : BotITime
{
    public void SetUtcNow(DateTime dateTime);
}

