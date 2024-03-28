using bot.src.Brokers;

namespace StrategyTester.src.Brokers.InMemory;

public class AccountOptions : IAccountOptions
{
    public decimal Balance { get; set; }
}
