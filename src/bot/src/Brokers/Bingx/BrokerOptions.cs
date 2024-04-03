namespace bot.src.Brokers.Bingx;

public class BrokerOptions : IBrokerOptions
{
    public int TimeFrame { get; set; }
    public decimal BrokerCommission { get; set; }
    public string Symbol { get; set; } = null!;
    public IAccountOptions AccountOptions { get; set; } = new AccountOptions();
    public string BaseUrl { get; set; } = null!;
    public string ApiKey { get; set; } = null!;
    public string ApiSecret { get; set; } = null!;
}
