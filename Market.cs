using System.Text.Json;
using System.Text.Json.Nodes;

namespace bingx_test;

public class Market : Api
{
    public Market(string base_url, string apiKey, string apiSecret, string symbol) : base(base_url, apiKey, apiSecret, symbol) { }

    public async Task<float> GetLastPrice(string symbol, int TimeFrame)
    {
        try
        {
            System.Console.WriteLine("\n\nGetting last price of the symbol...");

            if (symbol.Split("-", StringSplitOptions.RemoveEmptyEntries).Length != 2)
                throw new LastPriceException("Invalid Argument supplied.");
            HttpResponseMessage response = await new HttpClient().GetAsync($"https://min-api.cryptocompare.com/data/v2/histominute?fsym={symbol.Split("-")[0]}&tsym={symbol.Split("-")[1]}&limit=1&aggregate={TimeFrame}&e=binance");

            response.EnsureSuccessStatusCode();

            string responseString = await response.Content.ReadAsStringAsync();
            JsonNode responseBody = JsonNode.Parse(responseString) ?? throw new LastPriceException("Failed to parse response json.");

            if (responseBody["Data"]!["Data"]!.AsArray().Count != 2)
                throw new LastPriceException("Invalid response received.");

            float lastPrice = responseBody["Data"]!["Data"]![1]!["close"]!.GetValue<float>();

            System.Console.WriteLine("Got last symbol price...");
            return lastPrice;
        }
        catch (LastPriceException) { throw; }
        catch (Exception) { throw new LastPriceException(); }
    }
}