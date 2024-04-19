using Abstractions.src.Brokers;

namespace Brokers.src.InMemory;

public class AccountOptions : IAccountOptions
{
    public decimal Balance { get; set; }
}
