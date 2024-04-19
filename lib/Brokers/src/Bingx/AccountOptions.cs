using Abstractions.src.Brokers;

namespace Brokers.src.Bingx;

public class AccountOptions : IAccountOptions
{
    public decimal Balance { get; set; }
}
