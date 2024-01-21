using BingxBrokerOptions = bot.src.Brokers.Bingx.BrokerOptions;
using InMemoryBrokerOptions = bot.src.Brokers.InMemory.BrokerOptions;

namespace bot.src.Brokers;

public static class BrokerOptionsFactory
{
    public static IBrokerOptions CreateBrokerOptions(string brokerName) => brokerName switch
    {
        BrokerNames.BINGX => new BingxBrokerOptions(),
        BrokerNames.IN_MEMORY => new InMemoryBrokerOptions(),
        _ => throw new Exception()
    };
}
