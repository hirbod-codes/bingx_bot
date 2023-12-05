using System.Text.Json.Nodes;
using bingx_api.Exceptions;

namespace bingx_api;

public class Market : Api
{
    public Market(string base_url, string apiKey, string apiSecret, string symbol, Utilities utilities) : base(base_url, apiKey, apiSecret, symbol) => Utilities = utilities;

    private Utilities Utilities { get; set; }

    public async Task<float> GetLastPrice(string symbol, int timeFrame)
    {
        try
        {
            Utilities.Logger.Information("Getting last price of the symbol...");

            if (symbol.Split("-", StringSplitOptions.RemoveEmptyEntries).Length != 2)
                throw new LastPriceException("Invalid Argument supplied.");
            HttpResponseMessage response = await new HttpClient().GetAsync($"https://min-api.cryptocompare.com/data/v2/histominute?fsym={symbol.Split("-")[0]}&tsym={symbol.Split("-")[1]}&limit=1&aggregate={timeFrame}&e=binance");

            if (!response.IsSuccessStatusCode) throw new LastPriceException();

            string responseString = await response.Content.ReadAsStringAsync();
            JsonNode responseBody = JsonNode.Parse(responseString) ?? throw new LastPriceException("Failed to parse response json.");

            if (responseBody["Data"]!["Data"]!.AsArray().Count != 2)
                throw new LastPriceException("Invalid response received.");

            float lastPrice = responseBody["Data"]!["Data"]![1]!["close"]!.GetValue<float>();
            Utilities.Logger.Information($"last price => {lastPrice}");

            Utilities.Logger.Information("Got last symbol price...");
            return lastPrice;
        }
        catch (LastPriceException ex)
        {
            Utilities.Logger.Error(ex, "Failure while trying to get the {symbol} last price in {timeFrame} time frame.", symbol, timeFrame);
            throw;
        }
        catch (Exception ex)
        {
            Utilities.Logger.Error(ex, "Failure while trying to get the {symbol} last price in {timeFrame} time frame.", symbol, timeFrame);
            throw new LastPriceException();
        }
    }
}
