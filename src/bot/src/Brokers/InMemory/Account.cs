using Serilog;

namespace bot.src.Brokers.InMemory;

public class Account : IAccount
{
    private readonly AccountOptions _accountOptions;
    private readonly ILogger _logger;

    public Account(AccountOptions accountOptions, ILogger logger)
    {
        _accountOptions = accountOptions;
        _logger = logger;
    }

    public Task<decimal> GetBalance() => Task.FromResult(_accountOptions.Balance);
}
