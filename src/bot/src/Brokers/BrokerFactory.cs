using InMemoryTrade = bot.src.Broker.InMemory.Trade;
using BingxTrade = bot.src.Brokers.Bingx.Trade;
using Serilog;
using bot.src.Data;
using bot.src.Brokers.Bingx;
using InMemoryBroker = bot.src.Brokers.InMemory.Broker;
using BingxAccount = bot.src.Brokers.Bingx.Account;
using InMemoryAccount = bot.src.Brokers.InMemory.Account;
using BingxBrokerOptions = bot.src.Brokers.Bingx.BrokerOptions;
using InMemoryBrokerOptions = bot.src.Brokers.InMemory.BrokerOptions;

namespace bot.src.Brokers;

public static class BrokerFactory
{
    public static IBroker CreateBroker(string brokerName, InMemoryBrokerOptions brokerOptions, ITrade trade, IAccount account, ICandleRepository candleRepository, ILogger logger) => brokerName switch
    {
        "InMemory" => new InMemoryBroker(brokerOptions, trade, account, candleRepository, logger),
        _ => throw new Exception()
    };
    public static IBroker CreateBroker(string brokerName, BingxBrokerOptions brokerOptions, ITrade trade, IAccount account, ICandleRepository candleRepository, ILogger logger) => brokerName switch
    {
        _ => throw new Exception()
    };

    public static IAccount CreateAccount(string brokerName, BingxBrokerOptions brokerOptions, ILogger logger) => brokerName switch
    {
        "Bingx" => new BingxAccount(brokerOptions.BaseUrl, brokerOptions.ApiKey, brokerOptions.ApiSecret, brokerOptions.Symbol, new BingxUtilities(logger), logger),
        _ => throw new Exception()
    };

    public static IAccount CreateAccount(string brokerName, InMemoryBrokerOptions brokerOptions, ILogger logger) => brokerName switch
    {
        "InMemory" => new InMemoryAccount(brokerOptions.AccountOptions, logger),
        _ => throw new Exception()
    };

    public static ITrade CreateTrade(string brokerName, BingxBrokerOptions brokerOptions, ILogger logger) => brokerName switch
    {
        "Bingx" => new BingxTrade(brokerOptions.BaseUrl, brokerOptions.ApiKey, brokerOptions.ApiSecret, brokerOptions.Symbol, new BingxUtilities(logger), logger),
        _ => throw new Exception()
    };

    public static ITrade CreateTrade(string brokerName, InMemoryBrokerOptions brokerOptions, ICandleRepository candleRepository, IPositionRepository positionRepository, ILogger logger) => brokerName switch
    {
        "InMemory" => new InMemoryTrade(brokerOptions, candleRepository, positionRepository, logger),
        _ => throw new Exception()
    };
}
