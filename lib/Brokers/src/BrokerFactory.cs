using ILogger = Serilog.ILogger;
using Brokers.src.Bingx;
using BingxBroker = Brokers.src.Bingx.Broker;
using InMemoryBroker = Brokers.src.InMemory.Broker;
using BingxAccount = Brokers.src.Bingx.Account;
using Abstractions.src.Brokers;
using Abstractions.src.Utilities;
using Abstractions.src.Data;

namespace Brokers.src;

public static class BrokerFactory
{
    public static IBroker CreateBroker(string brokerName, IBrokerOptions brokerOptions, ILogger logger, ITime time, IPositionRepository? positionRepository) => brokerName switch
    {
        BrokerNames.BINGX => new BingxBroker(brokerOptions, new BingxUtilities(logger), logger, time),
        BrokerNames.IN_MEMORY => new InMemoryBroker(brokerOptions, positionRepository!, time, logger),
        _ => throw new Exception()
    };

    public static IAccount CreateAccount(string brokerName, IBrokerOptions brokerOptions, ILogger logger) => brokerName switch
    {
        BrokerNames.BINGX => new BingxAccount(brokerOptions, new BingxUtilities(logger), logger),
        _ => throw new Exception()
    };
}
