namespace bot.src.Brokers.Bingx;

public class BrokerOptions : IBrokerOptions
{
    public int TimeFrame { get; set; }
    public decimal BrokerCommission { get; set; }
    public string Symbol { get; set; } = null!;
    public AccountOptions AccountOptions { get; set; } = new AccountOptions();
    public string BaseUrl { get; set; } = null!;
    public string ApiKey { get; set; } = null!;
    public string ApiSecret { get; set; } = null!;

    public bool Equals(IBrokerOptions? other)
    {
        if (other == null || (other as BrokerOptions) == null) return false;

        BrokerOptions? o = other as BrokerOptions;

        if (o == null) return false;

        if (o.TimeFrame != TimeFrame) return false;
        if (o.BrokerCommission != BrokerCommission) return false;
        if (o.Symbol != Symbol) return false;
        if (o.BaseUrl != BaseUrl) return false;
        if (o.ApiKey != ApiKey) return false;
        if (o.ApiSecret != ApiSecret) return false;

        return true;
    }
}
