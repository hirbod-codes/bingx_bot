using ILogger = Serilog.ILogger;
using bot.src.Data;
using bot.src.Brokers.Bingx;
using BingxBroker = bot.src.Brokers.Bingx.Broker;
using BingxAccount = bot.src.Brokers.Bingx.Account;

namespace bot.src.Brokers;

public static class BrokerFactory
{
    public static IBroker CreateBroker(string brokerName, IBrokerOptions brokerOptions, ILogger logger, Util.ITime time) => brokerName switch
    {
        BrokerNames.BINGX => new BingxBroker(brokerOptions, new BingxUtilities(logger), logger, time),
        _ => throw new Exception()
    };

    public static IAccount CreateAccount(string brokerName, IBrokerOptions brokerOptions, ILogger logger) => brokerName switch
    {
        BrokerNames.BINGX => new BingxAccount(brokerOptions, new BingxUtilities(logger), logger),
        _ => throw new Exception()
    };
}
