namespace broker_api.src;

public interface IBingxUtilities
{
    public Task NotifyListeners(string message);
    public Task<HttpResponseMessage> HandleBingxRequest(string protocol, string host, string endpointAddress, string method, string apiKey, string apiSecret, object payload);
    public Task EnsureSuccessfulBingxResponse(HttpResponseMessage response);
    public Task<bool> TryEnsureSuccessfulBingxResponse(HttpResponseMessage response);
    public Task CalculateFinancialPerformance(DateTime startDateTime, ITrade trade);

}
