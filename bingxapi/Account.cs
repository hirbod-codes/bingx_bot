namespace bingx_test.BingxApi;

public class Account : Api
{
    public Account(string base_url, string apiKey, string apiSecret, string symbol) : base(base_url, apiKey, apiSecret, symbol) { }

    public async Task<HttpResponseMessage> GetBalance() => await Utilities.HandleBingxRequest("https", Base_Url, "/openApi/swap/v2/user/balance", "GET", ApiKey, ApiSecret, new { });
}
