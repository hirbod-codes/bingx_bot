using System.Security.Cryptography;
using System.Text.Json;
using bot.src.Brokers.Bingx.Exceptions;
using bot.src.Brokers.Bingx.Models;
using bot.src.Strategies.Models;
using Serilog;

namespace bot.src.Brokers.Bingx;

public class BingxUtilities : IBingxUtilities
{
    private readonly ILogger _logger;

    public BingxUtilities(ILogger logger) => _logger = logger.ForContext<BingxUtilities>();

    public async Task<HttpResponseMessage> HandleBingxRequest(string protocol, string host, string endpointAddress, string method, string apiKey, string apiSecret, object payload)
    {
        _logger.Information("Handling request to bingx...");

        long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        string parameters = $"timestamp={timestamp}";

        if (payload != null)
            foreach (var property in payload.GetType().GetProperties())
                parameters += $"&{property.Name}={property.GetValue(payload)}";

        string signature = CalculateHmacSha256(parameters, apiSecret);
        string url = $"{protocol}://{host}{endpointAddress}?{parameters}&signature={signature}";

        _logger.Information("timestamp: {timestamp}", timestamp);
        _logger.Information("signature: {signature}", signature);
        _logger.Information("{method} {url}", method, url);
        _logger.Information("payload: {@payload}", payload);

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

        _logger.Information("Request to bingx handled...");
        return response;
    }

    public async Task EnsureSuccessfulBingxResponse(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            _logger.Error("bingx response http status code is not successful, httpStatusCode: {httpStatusCode}, body: {responseBody}", response.StatusCode, await response.Content.ReadAsStringAsync());
            throw new BingxException($"bingx response http status code is not successful.");
        }

        string responseString = await response.Content.ReadAsStringAsync();
        BingxResponse bingxResponse = JsonSerializer.Deserialize<BingxResponse>(responseString, options: new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;

        if (bingxResponse!.Code != 0)
        {
            _logger.Error("bingx response code is not successful, code: {code}, body: {responseBody}", bingxResponse.Code, await response.Content.ReadAsStringAsync());
            throw new BingxException($"bingx response failure: {bingxResponse.Msg}");
        }
    }

    public async Task<bool> TryEnsureSuccessfulBingxResponse(HttpResponseMessage response)
    {
        try
        {
            if (!response.IsSuccessStatusCode)
            {
                _logger.Warning("bingx response http status code is not successful, httpStatusCode: {httpStatusCode}, body: {responseBody}", response.StatusCode, await response.Content.ReadAsStringAsync());
                return false;
            }

            string responseString = await response.Content.ReadAsStringAsync();
            BingxResponse bingxResponse = JsonSerializer.Deserialize<BingxResponse>(responseString, options: new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;

            if (bingxResponse!.Code != 0)
            {
                _logger.Warning("bingx response code is not successful, code: {code}, body: {responseBody}", bingxResponse.Code, await response.Content.ReadAsStringAsync());
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

    // class HistoricalPositions
    // {
    //     public IEnumerable<Position> Positions { get; set; } = null!;
    // }

    // class FinancialPerformanceReport
    // {
    //     public DateTime StartingDateTime { get; set; }
    //     public float TotalProfit { get; set; }
    //     public float Commission { get; set; }
    //     public int OrdersCount { get; set; }
    // }

    // public async Task CalculateFinancialPerformance(DateTime startDateTime, ITrade trade)
    // {
    //     Logger.Information("Calculating financial performance...");

    //     IEnumerable<Position> positions = await trade.GetAllPositions(startDateTime);
    //     if (!positions.Any())
    //     {
    //         Logger.Information("Finished calculating financial performance.(No orders found)");
    //         return;
    //     }

    //     IEnumerable<Position> filteredPositions = FilterCancelledPositions(positions);
    //     (float profit, float commission) = CalculateProfitAndCommission(filteredPositions);

    //     Logger.Information("Financial performance report: {FinancialPerformanceReport}", JsonSerializer.Serialize(new FinancialPerformanceReport()
    //     {
    //         StartingDateTime = startDateTime,
    //         TotalProfit = profit,
    //         Commission = commission,
    //         OrdersCount = positions.Count()
    //     }));

    //     Logger.Information("Finished calculating financial performance.");
    // }

    // private IEnumerable<Position> FilterCancelledPositions(IEnumerable<Position> positions)
    // {
    //     IEnumerable<Position> filteredPositions = Array.Empty<Position>();
    //     for (int i = 0; i < positions.Count(); i++)
    //     {
    //         if (positions.ElementAt(i).Status == "CANCELLED")
    //             continue;
    //         filteredPositions = filteredPositions.Append(positions.ElementAt(i));
    //     }

    //     return filteredPositions;
    // }

    // private (float profit, float commission) CalculateProfitAndCommission(IEnumerable<Position> positions)
    // {
    //     float totalProfit = 0, commission = 0;

    //     for (int i = 0; i < positions.Count(); i++)
    //     {
    //         if (positions.ElementAt(i).Status == "CANCELLED")
    //             continue;

    //         totalProfit += float.Parse(positions.ElementAt(i).Profit);
    //         totalProfit += float.Parse(positions.ElementAt(i).Commission);

    //         commission += float.Parse(positions.ElementAt(i).Commission);
    //     }

    //     if (commission < 0) commission *= -1;

    //     return (totalProfit, commission);
    // }
}
