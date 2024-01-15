using bot.src.Broker.InMemory;

namespace bot.src.Brokers.InMemory;

public class Account : IAccount
{
    private readonly AccountOptions _accountOptions;

    public Account(AccountOptions accountOptions)
    {
        _accountOptions = accountOptions;
    }

    public Task<decimal> GetBalance() => Task.FromResult(_accountOptions.Balance);
}
