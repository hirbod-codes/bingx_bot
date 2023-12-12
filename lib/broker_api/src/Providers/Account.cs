using System.Text.Json;

namespace broker_api.src.Providers;

public class Account : Api, IAccount
{
    public Account(string base_url, string apiKey, string apiSecret, string symbol, BingxUtilities utilities) : base(base_url, apiKey, apiSecret, symbol) => Utilities = utilities;
    private BingxUtilities Utilities { get; set; }

    public async Task<HttpResponseMessage> GetBalance() => await Utilities.HandleBingxRequest("https", Base_Url, "/openApi/swap/v2/user/balance", "GET", ApiKey, ApiSecret, new { });

    public async Task<int> GetOpenPositionCount()
    {
        HttpResponseMessage httpResponseMessage = await Utilities.HandleBingxRequest("https", Base_Url, "/openApi/swap/v2/user/positions", "GET", ApiKey, ApiSecret, new
        {
            symbol = Symbol,
        });
        await Utilities.EnsureSuccessfulBingxResponse(httpResponseMessage);
        string response = await httpResponseMessage.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement?>? dictionary = JsonSerializer.Deserialize<Dictionary<string, JsonElement?>>(response);

        return dictionary!["data"]!.Value.GetArrayLength();
    }
}
