using bot.src.Brokers;

namespace StrategyTester.src.Brokers.InMemory;

public class BrokerOptions : IBrokerOptions
{
    public decimal BrokerCommission { get; set; }
    public int TimeFrame { get; set; }
    public string Symbol { get; set; } = null!;
    public AccountOptions AccountOptions { get; set; } = null!;
}
