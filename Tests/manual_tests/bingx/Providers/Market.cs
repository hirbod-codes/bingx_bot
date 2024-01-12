using System.Text.Json;
using manual_tests.bingx.Exceptions;

namespace manual_tests.bingx.Providers;

public class Market : Api
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

    public async Task GetKLineData(string dir)
    {
        string url = "/openApi/swap/v3/quote/klines";
        url = "/openApi/swap/v2/user/balance";
        // url = "/openApi/swap/v2/quote/price";
        // url = "/openApi/swap/v2/trade/leverage";
        // url = "/openApi/swap/v2/trade/order";
        HttpResponseMessage httpResponseMessage = await Utilities.HandleBingxRequest("https", Base_Url, url, "GET", ApiKey, ApiSecret, new
        {
            // symbol = Symbol,
            // orderId = ""
            
            // timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()

            // interval = "1m",
            // startTime = DateTimeOffset.Parse(DateTime.UtcNow.AddYears(-1).ToString()).ToUnixTimeMilliseconds(),
            // endTime = DateTimeOffset.Parse(DateTime.UtcNow.AddYears(-1).AddHours(1).ToString()).ToUnixTimeMilliseconds(),
        });

        var t = await httpResponseMessage.Content.ReadAsStringAsync();

        File.WriteAllText($"{dir}/bingx/kline_data.json", t);
    }
}
