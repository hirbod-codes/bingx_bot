using BingxBrokerOptions = bot.src.Brokers.Bingx.BrokerOptions;

namespace bot.src.Brokers;

public static class BrokerOptionsFactory
{
    public static IBrokerOptions CreateBrokerOptions(string brokerName) => brokerName switch
    {
        BrokerNames.BINGX => new BingxBrokerOptions(),
        _ => throw new Exception()
    };

    public static Type? GetInstanceType(string? name) => name switch
    {
        null => null,
        BrokerNames.BINGX => typeof(BingxBrokerOptions),
        _ => throw new Exception()
    };
}
