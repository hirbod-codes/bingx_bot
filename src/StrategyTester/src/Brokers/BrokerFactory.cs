using Serilog;
using bot.src.Data;
using bot.src.Brokers;
using InMemoryBroker = StrategyTester.src.Brokers.InMemory.Broker;
using StrategyTester.src.Utils;

namespace StrategyTester.src.Brokers;

public static class BrokerFactory
{
    public static IBroker CreateBroker(string brokerName, IBrokerOptions brokerOptions, IPositionRepository positionRepository, ITime time, ILogger logger) => brokerName switch
    {
        BrokerNames.IN_MEMORY => new InMemoryBroker(brokerOptions, positionRepository, time, logger),
        _ => throw new Exception()
    };
}
