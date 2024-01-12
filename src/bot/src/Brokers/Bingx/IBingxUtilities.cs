namespace bot.src.Brokers.Bingx;

public interface IBingxUtilities
{
    public Task<HttpResponseMessage> HandleBingxRequest(string protocol, string host, string endpointAddress, string method, string apiKey, string apiSecret, object payload);
    public Task EnsureSuccessfulBingxResponse(HttpResponseMessage response);
    public Task<bool> TryEnsureSuccessfulBingxResponse(HttpResponseMessage response);
    // public Task CalculateFinancialPerformance(DateTime startDateTime, ITrade trade);

}
