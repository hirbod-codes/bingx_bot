namespace Abstractions.src.Brokers;

public interface IAccount
{
    public Task<decimal> GetBalance();
}
