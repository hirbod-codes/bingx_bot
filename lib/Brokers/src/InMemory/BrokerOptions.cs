using Abstractions.src.Brokers;

namespace Brokers.src.InMemory;

public class BrokerOptions : IBrokerOptions
{
    public decimal BrokerCommission { get; set; }
    public int TimeFrame { get; set; }
    public string Symbol { get; set; } = null!;
    public IAccountOptions AccountOptions { get; set; } = new AccountOptions();

    public bool Equals(IBrokerOptions? other)
    {
        if (other == null || (other as BrokerOptions) == null) return false;

        BrokerOptions? o = other as BrokerOptions;

        if (o == null) return false;

        if (o.TimeFrame != TimeFrame) return false;
        if (o.BrokerCommission != BrokerCommission) return false;
        if (o.Symbol != Symbol) return false;

        return true;
    }
}