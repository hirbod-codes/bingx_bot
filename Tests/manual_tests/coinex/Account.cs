using bot.src.Brokers;
using Serilog;

namespace manual_tests.coinex;

public class Account : Api, IAccount
{
    public ILogger Logger { get; }

    public Account(string base_url, string apiKey, string apiSecret, string symbol, ILogger logger) : base(base_url, apiKey, apiSecret, symbol) => Logger = logger;

    public async Task<float> GetBalance()
    {
        HttpResponseMessage httpResponseMessage = await HandleRequest(HttpMethod.Get, "perpetual/v1/asset/query", null);

        var t = await httpResponseMessage.Content.ReadAsStringAsync();

        return 0;
    }
}
