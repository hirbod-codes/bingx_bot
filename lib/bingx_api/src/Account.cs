namespace bingx_api;

public class Account : Api
{
    public Account(string base_url, string apiKey, string apiSecret, string symbol, Utilities utilities) : base(base_url, apiKey, apiSecret, symbol) => Utilities = utilities;
    private Utilities Utilities { get; set; }

    public async Task<HttpResponseMessage> GetBalance() => await Utilities.HandleBingxRequest("https", Base_Url, "/openApi/swap/v2/user/balance", "GET", ApiKey, ApiSecret, new { });
}
