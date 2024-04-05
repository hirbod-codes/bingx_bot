using bot.src.Brokers;
using ILogger = Serilog.ILogger;

namespace StrategyTester.src.Brokers.InMemory;

public class Account : IAccount
{
    private readonly IAccountOptions _accountOptions;
    private readonly ILogger _logger;

    public Account(IBrokerOptions brokerOptions, ILogger logger)
    {
        _accountOptions = (brokerOptions as BrokerOptions)!.AccountOptions;
        _logger = logger;
    }

    public Task<decimal> GetBalance() => Task.FromResult(_accountOptions.Balance);
}
