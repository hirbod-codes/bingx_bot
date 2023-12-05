using bingx_api;
using bingx_api.Exceptions;
using Microsoft.Extensions.Configuration;
using strategies.src;

namespace bot.src;

public class Bot : IBot
{
    public Bot(IConfigurationSection bingxApiConfSection, IStrategy strategy)
    {
        Strategy = strategy;
        Utilities = new(Program.Logger);

        BingxApi = bingxApiConfSection;

        Trade = new Trade(bingxApiConfSection["BaseUrl"]!, bingxApiConfSection["ApiKey"]!, bingxApiConfSection["ApiSecret"]!, bingxApiConfSection["Symbol"]!, Utilities);
        Market = new Market(bingxApiConfSection["BaseUrl"]!, bingxApiConfSection["ApiKey"]!, bingxApiConfSection["ApiSecret"]!, bingxApiConfSection["Symbol"]!, Utilities);
        TimeFrame = int.Parse(bingxApiConfSection["TimeFrame"]!);
        Margin = float.Parse(bingxApiConfSection["Margin"]!);
        Leverage = int.Parse(bingxApiConfSection["Leverage"]!);
        TpPercentage = !string.IsNullOrEmpty(bingxApiConfSection["TpPercentage"]) ? float.Parse(bingxApiConfSection["TpPercentage"]!) : null;
        SlPercentage = float.Parse(bingxApiConfSection["SlPercentage"]!);
    }

    public IConfigurationSection BingxApi { get; }

    private IStrategy Strategy { get; set; }
    private Utilities Utilities { get; set; }

    public Trade Trade { get; }
    public Market Market { get; }
    public int TimeFrame { get; }
    public float Margin { get; }
    public int Leverage { get; }
    public float LastPrice { get; private set; }
    public float? TpPercentage { get; } = null;
    public float SlPercentage { get; private set; } = 10f;

    private bool? IsLastOpenPositionLong { get; set; } = null;

    public async Task Run()
    {
        try
        {
            DateTime startDateTime = DateTime.UtcNow;

            Program.Logger.Information("Starting at {date}...", startDateTime);

            await Strategy.Initiate();

            (await Trade.SetLeverage(Leverage, true)).EnsureSuccessStatusCode();
            (await Trade.SetLeverage(Leverage, false)).EnsureSuccessStatusCode();

            while (true)
            {
                if (DateTime.UtcNow.Minute % TimeFrame == 0 && DateTime.UtcNow.Second == 0)
                {
                    Program.Logger.Information("\n\n--- Tick ---");
                    Program.Logger.Information("{tick}", DateTime.UtcNow);

                    try { await Utilities.NotifyListeners("Candle created."); }
                    catch (NotificationException) { throw; }

                    // 3 seconds delay to ensure the alert has reached gmail's severs
                    await Task.Delay(millisecondsDelay: 3000);

                    HttpResponseMessage response;

                    if (IsLastOpenPositionLong != null && Strategy.CheckClosePositionSignal(IsLastOpenPositionLong))
                    {
                        response = await Trade.CloseOpenPositions();
                        await Utilities.HandleBingxResponse(response);

                        try { await Utilities.CalculateFinancialPerformance(startDateTime, Trade); }
                        catch (System.Exception ex) { Program.Logger.Error(ex, "Failure while trying to calculate financial performance."); }
                    }

                    bool? signal = Strategy.CheckOpenPositionSignal(IsLastOpenPositionLong);
                    if (signal != null)
                    {
                        LastPrice = await Market.GetLastPrice(Trade.Symbol, TimeFrame);

                        bool isLong = (bool)signal;
                        response = await Trade.OpenMarketOrder(isLong, (float)(Margin * Leverage / LastPrice), TpPercentage != null ? Trade.CalculateTp(isLong, (float)TpPercentage, LastPrice, Leverage) : null, Trade.CalculateSl(isLong, SlPercentage, LastPrice, Leverage));
                        await Utilities.HandleBingxResponse(response);

                        IsLastOpenPositionLong = signal;
                    }
                }

                await Task.Delay(millisecondsDelay: 1000);
            }
        }
        catch (Exception ex)
        {
            Program.Logger.Fatal(ex, "Fatal exception has been thrown {message}", ex.Message);

            try { await Utilities.NotifyListeners($"Fatal Exception has been thrown: {ex.GetType().Name}, Message: {ex.Message}"); }
            catch (Exception NotifyListenersException) { Program.Logger.Error(NotifyListenersException, "Failure while trying to send notification"); }
            try { await Trade.CloseOpenPositions(); }
            catch (Exception closeOpenPositionsEx)
            {
                Program.Logger.Error(closeOpenPositionsEx, "Failure while trying to close all of the open positions.");
                try { await Utilities.NotifyListeners($"Fatal Exception has been thrown: {closeOpenPositionsEx.GetType().Name}, Message: {closeOpenPositionsEx.Message}"); }
                catch (Exception NotifyListenersException) { Program.Logger.Error(NotifyListenersException, "Failure while trying to send notification"); }
            }
            throw;
        }
    }
}
