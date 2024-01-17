namespace bot.src.Brokers.Bingx;

public class BrokerOptions
{
    public decimal BrokerCommission { get; set; }
    public string Symbol { get; set; } = null!;
    public AccountOptions AccountOptions { get; set; } = null!;
    public string BaseUrl { get; set; } = null!;
    public string ApiKey { get; set; } = null!;
    public string ApiSecret { get; set; } = null!;
}
