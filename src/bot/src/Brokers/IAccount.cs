namespace bot.src.Brokers;

public interface IAccount
{
    public Task<decimal> GetBalance();
}
