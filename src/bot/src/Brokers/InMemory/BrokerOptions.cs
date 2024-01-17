namespace bot.src.Brokers.InMemory;

public class BrokerOptions
{
    public decimal BrokerCommission { get; set; }
    public string Symbol { get; set; } = null!;
    public AccountOptions AccountOptions { get; set; } = null!;
}
