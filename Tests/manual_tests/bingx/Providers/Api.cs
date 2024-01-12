namespace manual_tests.bingx.Providers;

public class Api
{
    public string Base_Url { get; set; }
    public string ApiKey { get; set; }
    public string ApiSecret { get; set; }
    public string Symbol { get; set; }

    public Api(string base_url, string apiKey, string apiSecret, string symbol)
    {
        Base_Url = base_url;
        ApiKey = apiKey;
        ApiSecret = apiSecret;
        Symbol = symbol;
    }
}
