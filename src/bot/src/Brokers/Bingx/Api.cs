namespace bot.src.Brokers.Bingx;

public partial class Api
{
    public const string LONG_SIDE = "LONG";
    public const string SHORT_SIDE = "SHORT";

    public string Base_Url { get; set; }
    public string ApiKey { get; set; }
    public string ApiSecret { get; set; }
    public string Symbol { get; set; }

    public Api(IBrokerOptions brokerOptions)
    {
        BrokerOptions opt = (brokerOptions as BrokerOptions)!;
        Base_Url = opt.BaseUrl;
        ApiKey = opt.ApiKey;
        ApiSecret = opt.ApiSecret;
        Symbol = opt.Symbol;
    }
}
