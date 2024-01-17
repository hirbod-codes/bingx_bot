using bot.src.Broker;

namespace bot.src.Brokers;

public interface IBrokerFactory
{
    public IBroker CreateBroker();
    public ITrade CreateTrade();
}
