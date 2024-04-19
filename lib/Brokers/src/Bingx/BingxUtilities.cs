using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using Brokers.src.Bingx.Exceptions;
using Brokers.src.Bingx.Models;
using ILogger = Serilog.ILogger;

namespace Brokers.src.Bingx;

public class BingxUtilities : IBingxUtilities
{
    private readonly ILogger _logger;

    public BingxUtilities(ILogger logger) => _logger = logger.ForContext<BingxUtilities>();

    public async Task<HttpResponseMessage> HandleBingxRequest(string protocol, string host, string endpointAddress, string method, string apiKey, string apiSecret, object payload)
    {
        _logger.Information("Handling request to bingx...");

        using HttpClient clientTemp = new();
        HttpResponseMessage serverTimeResponse = await clientTemp.GetAsync($"{protocol}://{host}/openApi/swap/v2/server/time");

        if (!await TryEnsureSuccessfulBingxResponse(serverTimeResponse))
            throw new BingxServerTimeException();

        string serverTimeResponseJson = await serverTimeResponse.Content.ReadAsStringAsync();
        BingxResponse<BingxServerTime> bingxServerTime;
        try
        {
            bingxServerTime = JsonSerializer.Deserialize<BingxResponse<BingxServerTime>>(serverTimeResponseJson, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new BingxException("Failure while trying to fetch historical candles.");
        }
        catch (System.Exception ex)
        {
            _logger.Error(ex, "The broker failed: {message}", ex.Message);
            _logger.Information("The response: {response}", serverTimeResponseJson);
            throw new BingxServerTimeException();
        }

        long timestamp = bingxServerTime.Data!.ServerTime;

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

    public async Task<byte[]> DecompressBytes(byte[] bytes) => await DecompressBytesAsync(bytes);
    public async Task<byte[]> DecompressBytesAsync(byte[] bytes, CancellationToken cancel = default)
    {
        using var inputStream = new MemoryStream(bytes);
        using var outputStream = new MemoryStream();
        using (var compressionStream = new GZipStream(inputStream, CompressionMode.Decompress))
            await compressionStream.CopyToAsync(outputStream, cancel);

        return outputStream.ToArray();
    }
}
