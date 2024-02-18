using bot.src.Brokers;
using StrategyTester.src.Brokers.InMemory;

namespace StrategyTester.src.Brokers;

public static class BrokerOptionsFactory
{
    public static IBrokerOptions CreateBrokerOptions(string brokerName) => brokerName switch
    {
        BrokerNames.IN_MEMORY => new BrokerOptions(),
        _ => throw new Exception()
    };
}
