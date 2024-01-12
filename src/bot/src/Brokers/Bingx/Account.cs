using System.Text.Json;
using bot.src.Brokers.Bingx.Exceptions;
using bot.src.Brokers.Bingx.Models;

namespace bot.src.Brokers.Bingx;

public class Account : Api, IAccount
{
    private BingxUtilities Utilities { get; set; }

    public Account(string base_url, string apiKey, string apiSecret, string symbol, BingxUtilities utilities) : base(base_url, apiKey, apiSecret, symbol) => Utilities = utilities;

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
        public float Value { get; set; }
        public float Equity { get; set; }
        public float UnrealizedProfit { get; set; }
        public float RealizedProfit { get; set; }
        public float AvailableMargin { get; set; }
        public float UsedMargin { get; set; }
        public float FreezedMargin { get; set; }
    }

    public async Task<float> GetBalance()
    {
        Utilities.Logger.Information("Getting account balance...");

        HttpResponseMessage httpResponseMessage = await Utilities.HandleBingxRequest("https", Base_Url, "/openApi/swap/v2/user/balance", "GET", ApiKey, ApiSecret, new
        {
            symbol = Symbol,
        });

        if (!await Utilities.TryEnsureSuccessfulBingxResponse(httpResponseMessage))
            throw new AccountBalanceException();

        string response = await httpResponseMessage.Content.ReadAsStringAsync();

        if (!float.TryParse(((JsonSerializer.Deserialize<BingxResponse<BingxBalance>>(response, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new AccountBalanceException()).Data ?? throw new AccountBalanceException()).Balance ?? throw new AccountBalanceException(), out float balance))
            throw new AccountBalanceException();

        Utilities.Logger.Information("account balance => {balance}", balance);

        Utilities.Logger.Information("Finished getting account balance...");
        return balance;
    }
}
