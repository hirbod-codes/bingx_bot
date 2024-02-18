using BotIBroker = bot.src.Brokers.IBroker;

namespace StrategyTester.src.Brokers;

public interface IBroker : BotIBroker
{
    public void NextCandle();
    public bool IsFinished();
}
