using manual_tests.bingx.Providers;

namespace manual_tests.bingx;

public interface IBingxUtilities
{
    public Task NotifyListeners(string message);
    public Task<HttpResponseMessage> HandleBingxRequest(string protocol, string host, string endpointAddress, string method, string apiKey, string apiSecret, object payload);
    public Task EnsureSuccessfulBingxResponse(HttpResponseMessage response);
    public Task<bool> TryEnsureSuccessfulBingxResponse(HttpResponseMessage response);
    public Task CalculateFinancialPerformance(DateTime startDateTime, Trade trade);

}
