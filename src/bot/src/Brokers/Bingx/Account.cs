using System.Text.Json;
using bot.src.Brokers.Bingx.Exceptions;
using bot.src.Brokers.Bingx.Models;
using Serilog;

namespace bot.src.Brokers.Bingx;

public class Account : Api, IAccount
{
    private IBingxUtilities Utilities { get; set; }
    private readonly ILogger _logger;

    public Account(IBrokerOptions brokerOptions, IBingxUtilities utilities, ILogger logger) : base(brokerOptions)
    {
        Utilities = utilities;
        _logger = logger;
    }

    class BingxBalance
    {
        public string UserId { get; set; } = null!;
        public string Asset { get; set; } = null!;
        public string Balance { get; set; } = null!;
        public string Equity { get; set; } = null!;
        public string UnrealizedProfit { get; set; } = null!;
        public string RealisedProfit { get; set; } = null!;
        public string AvailableMargin { get; set; } = null!;
        public string UsedMargin { get; set; } = null!;
        public string FreezedMargin { get; set; } = null!;
    }

    class Balance
    {
        public string UserId { get; set; } = null!;
        public string Asset { get; set; } = null!;
        public decimal Value { get; set; }
        public decimal Equity { get; set; }
        public decimal UnrealizedProfit { get; set; }
        public decimal RealizedProfit { get; set; }
        public decimal AvailableMargin { get; set; }
        public decimal UsedMargin { get; set; }
        public decimal FreezedMargin { get; set; }
    }

    public async Task<decimal> GetBalance()
    {
        _logger.Information("Getting account balance...");

        HttpResponseMessage httpResponseMessage = await Utilities.HandleBingxRequest("https", Base_Url, "/openApi/swap/v2/user/balance", "GET", ApiKey, ApiSecret, new
        {
            symbol = Symbol,
        });

        if (!await Utilities.TryEnsureSuccessfulBingxResponse(httpResponseMessage))
            throw new AccountBalanceException();

        string response = await httpResponseMessage.Content.ReadAsStringAsync();

        if (!decimal.TryParse(((JsonSerializer.Deserialize<BingxResponse<BingxBalance>>(response, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new AccountBalanceException()).Data ?? throw new AccountBalanceException()).Balance ?? throw new AccountBalanceException(), out decimal balance))
            throw new AccountBalanceException();

        _logger.Information("account balance => {balance}", balance);

        _logger.Information("Finished getting account balance...");
        return balance;
    }
}
