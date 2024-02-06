using InMemoryTrade = bot.src.Broker.InMemory.Trade;
using BingxTrade = bot.src.Brokers.Bingx.Trade;
using Serilog;
using bot.src.Data;
using bot.src.Brokers.Bingx;
using InMemoryBroker = bot.src.Brokers.InMemory.Broker;
using BingxBroker = bot.src.Brokers.Bingx.Broker;
using BingxAccount = bot.src.Brokers.Bingx.Account;
using InMemoryAccount = bot.src.Brokers.InMemory.Account;

namespace bot.src.Brokers;

public static class BrokerFactory
{
    public static IBroker CreateBroker(string brokerName, IBrokerOptions brokerOptions, ITrade trade, IAccount account, IPositionRepository positionRepository, ICandleRepository candleRepository, ILogger logger) => brokerName switch
    {
        BrokerNames.IN_MEMORY => new InMemoryBroker(brokerOptions, trade, account, positionRepository, candleRepository, logger),
        BrokerNames.BINGX => new BingxBroker(brokerOptions, trade, new BingxUtilities(logger), logger),
        _ => throw new Exception()
    };

    public static IAccount CreateAccount(string brokerName, IBrokerOptions brokerOptions, ILogger logger) => brokerName switch
    {
        BrokerNames.IN_MEMORY => new InMemoryAccount(brokerOptions, logger),
        BrokerNames.BINGX => new BingxAccount(brokerOptions, new BingxUtilities(logger), logger),
        _ => throw new Exception()
    };

    public static ITrade CreateTrade(string brokerName, IBrokerOptions brokerOptions, ICandleRepository? candleRepository, IPositionRepository? positionRepository, ILogger logger) => brokerName switch
    {
        BrokerNames.BINGX => new BingxTrade(brokerOptions, new BingxUtilities(logger), logger),
        BrokerNames.IN_MEMORY => new InMemoryTrade(brokerOptions, candleRepository!, positionRepository!, logger),
        _ => throw new Exception()
    };
}
