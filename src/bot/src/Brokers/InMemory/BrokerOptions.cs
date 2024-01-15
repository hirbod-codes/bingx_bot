
namespace bot.src.Broker.InMemory;

public record BrokerOptions
{
    public decimal BrokerCommission { get; set; }
    public string Symbol { get; set; } = null!;
    public AccountOptions AccountOptions { get; set; } = null!;
}
