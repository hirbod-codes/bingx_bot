using BingxBrokerOptions = bot.src.Brokers.Bingx.BrokerOptions;

namespace bot.src.Brokers;

public static class BrokerOptionsFactory
{
    public static IBrokerOptions CreateBrokerOptions(string brokerName) => brokerName switch
    {
        BrokerNames.BINGX => new BingxBrokerOptions(),
        _ => throw new Exception()
    };
}
