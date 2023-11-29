namespace bingx_test;

public class OrderManagement : Api
{
    public OrderManagement(string base_url, string apiKey, string apiSecret, string symbol) : base(base_url, apiKey, apiSecret, symbol) { }

    public async Task<HttpResponseMessage> GetOrders() => await Utilities.HandleRequest("https", Base_Url, "/openApi/swap/v2/trade/openOrders", "GET", ApiKey, ApiSecret, new
    {
        symbol = Symbol
    });

    public async Task<HttpResponseMessage> DeleteOrders() => await Utilities.HandleRequest("https", Base_Url, "/openApi/swap/v2/trade/allOpenOrders", "DELETE", ApiKey, ApiSecret, new
    {
        symbol = Symbol
    });

    public async Task<HttpResponseMessage> DeleteOrders(IEnumerable<long> orderIds) => await Utilities.HandleRequest("https", Base_Url, "/openApi/swap/v2/trade/batchOrders", "DELETE", ApiKey, ApiSecret, new
    {
        orderIdList = orderIds,
        symbol = Symbol
    });
}
