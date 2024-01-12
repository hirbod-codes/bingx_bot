using System.Security.Cryptography;
using System.Text;
using manual_tests.coinex.Exceptions;

namespace manual_tests.coinex;

public class Api
{
    public string BaseUrl { get; set; }
    public string ApiKey { get; set; }
    public string ApiSecret { get; set; }
    public string Symbol { get; set; }

    public Api(string base_url, string apiKey, string apiSecret, string symbol)
    {
        if (string.IsNullOrEmpty(base_url) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret) || string.IsNullOrEmpty(symbol))
            throw new CoinexException();

        BaseUrl = base_url;
        ApiKey = apiKey;
        ApiSecret = apiSecret;
        Symbol = symbol;
    }

    protected async Task<HttpResponseMessage> HandleRequest(HttpMethod method, string path, Dictionary<string, object>? parameters = null)
    {
        parameters ??= new(Array.Empty<KeyValuePair<string, object>>());
        HttpClient httpClient = new();

        HttpRequestMessage req = null!;
        if (method.Method == HttpMethod.Get.Method)
        {
            string signedBody = Sign(parameters);

            req = new(HttpMethod.Get, $"https://api.coinex.com/{path}?{string.Join("&", parameters.Select(p => $"{p.Key}={p.Value}"))}");
            req.Headers.Add("authorization", signedBody);
        }
        else
        {
        }

        req.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.75 Safari/537.36");
        HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(req);

        return httpResponseMessage;
    }

    private string Sign(Dictionary<string, object> parameters)
    {
        parameters["access_id"] = ApiKey;
        parameters["tonce"] = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;

        var last_args = new[] { new KeyValuePair<string, object>("secret_key", ApiSecret) };
        IEnumerable<KeyValuePair<string, object>> sortedArgs = parameters.OrderBy(p => p.Key).Concat(last_args);
        string queryVariables = string.Join("&", sortedArgs.Select(p => $"{p.Key}={p.Value}"));

        byte[] md5 = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(queryVariables));
        return BitConverter.ToString(md5).Replace("-", string.Empty).ToUpper();
    }
}
