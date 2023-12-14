using System.Text.Json;
using broker_api.Exceptions;

namespace broker_api.src.Providers;

public class Market : Api, IMarket
{
    public Market(string base_url, string apiKey, string apiSecret, string symbol, BingxUtilities utilities) : base(base_url, apiKey, apiSecret, symbol) => Utilities = utilities;

    private BingxUtilities Utilities { get; set; }

    public async Task<float> GetLastPrice(string symbol, int timeFrame)
    {
        try
        {
            Utilities.Logger.Information("Getting last price of the symbol...");

            HttpResponseMessage httpResponseMessage = await Utilities.HandleBingxRequest("https", Base_Url, "/openApi/swap/v2/quote/price", "GET", ApiKey, ApiSecret, new
            {
                symbol = Symbol,
            });
            await Utilities.EnsureSuccessfulBingxResponse(httpResponseMessage);

            string response = await httpResponseMessage.Content.ReadAsStringAsync();

            Dictionary<string, JsonElement?>? dictionary = JsonSerializer.Deserialize<Dictionary<string, JsonElement?>>(response);

            Dictionary<string, JsonElement?>? data = JsonSerializer.Deserialize<Dictionary<string, JsonElement?>>(dictionary!["data"]!.Value);

            float lastPrice = float.Parse(data!["price"]!.Value.GetString()!);
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
