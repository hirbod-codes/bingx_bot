using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using bingx_api.Exceptions;
using bingx_api.src.Models;
using Serilog.Core;

namespace bingx_api;

public class Utilities
{
    public Logger Logger { get; set; }

    public Utilities(Logger logger) => Logger = logger;

    public async Task NotifyListeners(string message)
    {
        Logger.Information("Sending notification...");
        Logger.Information("Notification message: {message}");
        HttpResponseMessage response = await new HttpClient().PostAsync("http://ntfy.sh/xpSQ6aicPmPB38VV1653rq", new StringContent(message));
        Logger.Information("Notification has been sent...");

        if (!response.IsSuccessStatusCode) throw new NotificationException();
    }

    public async Task<HttpResponseMessage> HandleBingxRequest(string protocol, string host, string endpointAddress, string method, string apiKey, string apiSecret, object payload)
    {
        Logger.Information("Handling request to bingx...");

        long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        string parameters = $"timestamp={timestamp}";

        if (payload != null)
            foreach (var property in payload.GetType().GetProperties())
                parameters += $"&{property.Name}={property.GetValue(payload)}";

        string signature = CalculateHmacSha256(parameters, apiSecret);
        string url = $"{protocol}://{host}{endpointAddress}?{parameters}&signature={signature}";

        Logger.Information("timestamp: {timestamp}", timestamp);
        Logger.Information("signature: {signature}", signature);
        Logger.Information("{method} {url}", method, url);
        Logger.Information("payload: {@payload}", payload);

        using HttpClientHandler handler = new();
        handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

        using HttpClient client = new(handler);
        client.DefaultRequestHeaders.Add("X-BX-APIKEY", apiKey);

        HttpResponseMessage response = method.ToUpper() switch
        {
            "GET" => await client.GetAsync(url),
            "POST" => await client.PostAsync(url, null),
            "DELETE" => await client.DeleteAsync(url),
            "PUT" => await client.PutAsync(url, null),
            _ => throw new NotSupportedException("Unsupported HTTP method: " + method),
        };

        Logger.Information("Request to bingx handled...");
        return response;
    }

    public async Task<string> HandleBingxResponse(HttpResponseMessage response)
    {
        Logger.Information("Handling response to bingx...");

        string responseString = await response.Content.ReadAsStringAsync();
        Logger.Information("Response http status code: {httpStatusCode}", response.StatusCode);
        Logger.Information("bingx response: {@response}", responseString);

        if (!response.IsSuccessStatusCode) throw new ResponseHandlingException();

        JsonNode responseBody = JsonNode.Parse(responseString) ?? throw new Exception();

        if (responseBody["code"] == null || responseBody["code"]!.GetValue<int>() != 0)
            throw new Exception();

        Logger.Information("Response to bingx handled...");
        return responseString;
    }

    public async Task EnsureSuccessfulBingxResponse(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            Logger.Error("bingx response http status code is not successful, httpStatusCode: {httpStatusCode}, body: {responseBody}", response.StatusCode, await response.Content.ReadAsStringAsync());
            throw new Exception($"bingx response http status code is not successful.");
        }

        string responseString = await response.Content.ReadAsStringAsync();
        BingxResponse bingxResponse = JsonSerializer.Deserialize<BingxResponse>(responseString)!;

        if (bingxResponse!.Code == 0)
        {
            Logger.Error("bingx response code is not successful, code: {code}, body: {responseBody}", bingxResponse.Code, await response.Content.ReadAsStringAsync());
            throw new Exception($"bingx response failure: {bingxResponse.Msg}");
        }
    }

    public async Task<bool> TryEnsureSuccessfulBingxResponse(HttpResponseMessage response)
    {
        try
        {
            if (!response.IsSuccessStatusCode)
            {
                Logger.Warning("bingx response http status code is not successful, httpStatusCode: {httpStatusCode}, body: {responseBody}", response.StatusCode, await response.Content.ReadAsStringAsync());
                return false;
            }

            string responseString = await response.Content.ReadAsStringAsync();
            BingxResponse bingxResponse = JsonSerializer.Deserialize<BingxResponse>(responseString)!;

            if (bingxResponse!.Code == 0)
            {
                Logger.Warning("bingx response code is not successful, code: {code}, body: {responseBody}", bingxResponse.Code, await response.Content.ReadAsStringAsync());
                return false;
            }

            return true;
        }
        catch (Exception) { return false; }
    }

    private string CalculateHmacSha256(string input, string key)
    {
        byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(key);
        byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
        using HMACSHA256 hmac = new(keyBytes);
        byte[] hashBytes = hmac.ComputeHash(inputBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    class HistoryData
    {
        public IEnumerable<Order> Orders { get; set; } = null!;
    }

    class FinancialPerformanceReport
    {
        public DateTime StartingDateTime { get; set; }
        public float TotalProfit { get; set; }
        public float Commission { get; set; }
        public int OrdersCount { get; set; }
    }

    public async Task CalculateFinancialPerformance(DateTime startDateTime, Trade trade)
    {
        Logger.Information("Calculating financial performance...");

        HttpResponseMessage response = await trade.GetOrders(startDateTime, DateTime.UtcNow);
        string json = await HandleBingxResponse(response);
        BingxResponse<HistoryData>? bingxResponse = JsonSerializer.Deserialize<BingxResponse<HistoryData>>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

        IEnumerable<Order> filteredOrders = FilterCancelledOrders(bingxResponse!.Data!.Orders);
        (float profit, float commission) = CalculateProfitAndCommission(filteredOrders);

        Logger.Information("Financial performance report: {FinancialPerformanceReport}", new FinancialPerformanceReport()
        {
            StartingDateTime = startDateTime,
            TotalProfit = profit,
            Commission = commission,
            OrdersCount = bingxResponse!.Data!.Orders.Count()
        });

        Logger.Information("Finished calculating financial performance...");
    }

    private IEnumerable<Order> FilterCancelledOrders(IEnumerable<Order> orders)
    {
        IEnumerable<Order> filteredOrders = Array.Empty<Order>();
        for (int i = 0; i < orders.Count(); i++)
        {
            if (orders.ElementAt(i).Status == "CANCELLED")
                continue;
            filteredOrders = filteredOrders.Append(orders.ElementAt(i));
        }

        return filteredOrders;
    }

    private (float profit, float commission) CalculateProfitAndCommission(IEnumerable<Order> orders)
    {
        float totalProfit = 0, commission = 0;

        for (int i = 0; i < orders.Count(); i++)
        {
            if (orders.ElementAt(i).Status == "CANCELLED")
                continue;

            totalProfit += float.Parse(orders.ElementAt(i).Profit);
            totalProfit += float.Parse(orders.ElementAt(i).Commission);

            commission += float.Parse(orders.ElementAt(i).Commission);
        }

        if (commission < 0) commission *= -1;

        return (totalProfit, commission);
    }
}
