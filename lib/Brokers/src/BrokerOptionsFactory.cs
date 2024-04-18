using Abstractions.src.Brokers;
using BingxBrokerOptions = Brokers.src.Bingx.BrokerOptions;
using InMemoryBrokerOptions = Brokers.src.InMemory.BrokerOptions;

namespace Brokers.src;

public static class BrokerOptionsFactory
{
    public static IBrokerOptions CreateBrokerOptions(string brokerName) => brokerName switch
    {
        BrokerNames.IN_MEMORY => new InMemoryBrokerOptions(),
        BrokerNames.BINGX => new BingxBrokerOptions(),
        _ => throw new Exception()
    };

    public static Type? GetInstanceType(string? name) => name switch
    {
        null => null,
        BrokerNames.BINGX => typeof(BingxBrokerOptions),
        BrokerNames.IN_MEMORY => typeof(InMemoryBrokerOptions),
        _ => throw new Exception()
    };
}
