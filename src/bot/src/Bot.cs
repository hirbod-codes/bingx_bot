using bingx_api;
using bingx_api.Exceptions;
using gmail_api.Exceptions;
using Microsoft.Extensions.Configuration;
using strategies.src;

namespace bot.src;

public class Bot : IBot
{
    public Bot(IConfigurationSection bingxApiConfSection, IStrategy eMAStrategy)
    {
        BingxApi = bingxApiConfSection;

        Trade = new Trade(bingxApiConfSection["BaseUrl"]!, bingxApiConfSection["ApiKey"]!, bingxApiConfSection["ApiSecret"]!, bingxApiConfSection["Symbol"]!);
        Market = new Market(bingxApiConfSection["BaseUrl"]!, bingxApiConfSection["ApiKey"]!, bingxApiConfSection["ApiSecret"]!, bingxApiConfSection["Symbol"]!);
        TimeFrame = int.Parse(bingxApiConfSection["TimeFrame"]!);
        Margin = float.Parse(bingxApiConfSection["Margin"]!);
        Leverage = int.Parse(bingxApiConfSection["Leverage"]!);
        TpPercentage = !string.IsNullOrEmpty(bingxApiConfSection["TpPercentage"]) ? float.Parse(bingxApiConfSection["TpPercentage"]!) : null;
        SlPercentage = float.Parse(bingxApiConfSection["SlPercentage"]!);

        EMAStrategy = eMAStrategy;
    }

    public IConfigurationSection BingxApi { get; }

    private IStrategy EMAStrategy { get; set; }

    public Trade Trade { get; }
    public Market Market { get; }
    public int TimeFrame { get; }
    public float Margin { get; }
    public int Leverage { get; }
    public float LastPrice { get; private set; }
    public float? TpPercentage { get; } = null;
    public float SlPercentage { get; private set; } = 10f;

    private bool? IsCurrentOpenPositionLong { get; set; } = null;

    public async Task Run()
    {
        try
        {
            System.Console.WriteLine("Starting...");

            DateTime startDateTime = DateTime.UtcNow;

            await EMAStrategy.Initiate();

            (await Trade.SetLeverage(Leverage, true)).EnsureSuccessStatusCode();
            (await Trade.SetLeverage(Leverage, false)).EnsureSuccessStatusCode();

            while (true)
            {
                if (DateTime.UtcNow.Minute % TimeFrame == 0 && DateTime.UtcNow.Second == 0)
                {
                    System.Console.WriteLine("--- Tick ---");
                    System.Console.WriteLine($"Minute ==> {DateTime.UtcNow.Minute}");

                    await Utilities.NotifyListeners("Candle created.");

                    // 3 seconds delay to ensure the alert has reached gmail's severs
                    await Task.Delay(millisecondsDelay: 3000);

                    HttpResponseMessage response;

                    if (IsCurrentOpenPositionLong != null && EMAStrategy.CheckClosePositionSignal(IsCurrentOpenPositionLong))
                    {
                        response = await Trade.CloseOpenPositions();
                        await Utilities.HandleBingxResponse(response);
                        IsCurrentOpenPositionLong = null;

                        response = await Trade.GetOrders(startDateTime, DateTime.UtcNow);
                        Utilities.CalculateProfit(await Utilities.HandleBingxResponse(response));
                    }

                    if (IsCurrentOpenPositionLong == null)
                    {
                        bool? signal = EMAStrategy.CheckOpenPositionSignal(IsCurrentOpenPositionLong);
                        if (signal != null)
                        {
                            LastPrice = await Market.GetLastPrice(Trade.Symbol, TimeFrame);

                            bool isLong = (bool)signal;
                            response = await Trade.OpenMarketOrder(isLong, (float)(Margin * Leverage / LastPrice), TpPercentage != null ? Trade.CalculateTp(isLong, (float)TpPercentage, LastPrice, Leverage) : null, Trade.CalculateSl(isLong, SlPercentage, LastPrice, Leverage));
                            await Utilities.HandleBingxResponse(response);

                            IsCurrentOpenPositionLong = signal;
                        }
                    }
                }

                await Task.Delay(millisecondsDelay: 1000);
            }
        }
        catch (NotificationException) { throw; }
        catch (BingxApiException ex)
        {
            try { await Utilities.NotifyListeners($"Fatal Exception has been thrown: {ex.GetType().Name}, Message: {ex.Message}"); }
            catch (Exception) { }
            try { await Trade.CloseOpenPositions(); }
            catch (Exception closeOpenPositionsEx)
            {
                try { await Utilities.NotifyListeners($"Fatal Exception has been thrown: {closeOpenPositionsEx.GetType().Name}, Message: {closeOpenPositionsEx.Message}"); }
                catch (Exception) { }
            }
            throw;
        }
        catch (GmailApiException ex)
        {
            try { await Utilities.NotifyListeners($"Fatal Exception has been thrown: {ex.GetType().Name}, Message: {ex.Message}"); }
            catch (Exception) { }
            try { await Trade.CloseOpenPositions(); }
            catch (Exception closeOpenPositionsEx)
            {
                try { await Utilities.NotifyListeners($"Fatal Exception has been thrown: {closeOpenPositionsEx.GetType().Name}, Message: {closeOpenPositionsEx.Message}"); }
                catch (Exception) { }
            }
            throw;
        }
        catch (Exception ex)
        {
            try { await Utilities.NotifyListeners($"Fatal Exception has been thrown: {ex.GetType().Name}, Message: {ex.Message}"); }
            catch (Exception) { }
            throw;
        }
    }
}
