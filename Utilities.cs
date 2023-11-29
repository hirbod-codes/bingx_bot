using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace bingx_test;

public static class Utilities
{
    public static async Task NotifyListeners()
    {
        System.Console.WriteLine("\n\nSending notification on candle creation...");
        await new HttpClient().PostAsync("http://ntfy.sh/xpSQ6aicPmPB38VV1653rq", new StringContent("Candle created."));
        System.Console.WriteLine("Notification has been sent...");
    }

    public static async Task<HttpResponseMessage> HandleRequest(string protocol, string host, string endpointAddress, string method, string apiKey, string apiSecret, object payload)
    {
        System.Console.WriteLine("\n\nHandling request to bingx...");

        long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        string parameters = $"timestamp={timestamp}";

        if (payload != null)
            foreach (var property in payload.GetType().GetProperties())
                parameters += $"&{property.Name}={property.GetValue(payload)}";

        string sign = CalculateHmacSha256(parameters, apiSecret);
        string url = $"{protocol}://{host}{endpointAddress}?{parameters}&signature={sign}";

        Console.WriteLine("payload: " + JsonSerializer.Serialize(payload));
        Console.WriteLine(method + " " + url);

        using HttpClientHandler handler = new();
        handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

        using HttpClient client = new(handler);
        client.DefaultRequestHeaders.Add("X-BX-APIKEY", apiKey);

        HttpResponseMessage httpResponseMessage = method.ToUpper() switch
        {
            "GET" => await client.GetAsync(url),
            "POST" => await client.PostAsync(url, null),
            "DELETE" => await client.DeleteAsync(url),
            "PUT" => await client.PutAsync(url, null),
            _ => throw new NotSupportedException("Unsupported HTTP method: " + method),
        };

        System.Console.WriteLine("\n\nRequest to bingx handled...");
        return httpResponseMessage;
    }

    public static async Task<JsonNode> HandleResponse(HttpResponseMessage response)
    {
        System.Console.WriteLine("\n\nHandling response to bingx...");

        string responseString = await response.Content.ReadAsStringAsync();
        Console.WriteLine("Response status code: " + response.StatusCode);
        Console.WriteLine("Response body: " + responseString);

        await File.WriteAllTextAsync("./response.json", responseString);

        response.EnsureSuccessStatusCode();

        JsonNode responseBody = JsonNode.Parse(responseString) ?? throw new Exception();

        if (responseBody["code"] == null || responseBody["code"]!.GetValue<int>() != 0)
            throw new Exception();

        System.Console.WriteLine("\n\nResponse to bingx handled...");
        return responseBody;
    }

    private static string CalculateHmacSha256(string input, string key)
    {
        byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(key);
        byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
        using HMACSHA256 hmac = new HMACSHA256(keyBytes);
        byte[] hashBytes = hmac.ComputeHash(inputBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}