using broker_api.Exceptions;
using email_api.src;
using broker_api.src;

namespace bot.src;

public class Bot : IBot
{
    public Bot(IStrategy strategy, IAccount account, ITrade trade, IMarket market, int timeFrame, IBingxUtilities bingxUtilities, IUtilities utilities)
    {
        Strategy = strategy;
        BingxUtilities = bingxUtilities;
        Utilities = utilities;

        Account = account;
        Trade = trade;
        Market = market;
        TimeFrame = timeFrame;
    }

    private IStrategy Strategy { get; set; }
    public IUtilities Utilities { get; private set; }
    private IBingxUtilities BingxUtilities { get; set; }

    public IAccount Account { get; }
    public ITrade Trade { get; }
    public IMarket Market { get; }
    public int TimeFrame { get; }
    public float LastPrice { get; private set; }

    private bool? IsLastOpenPositionLong { get; set; } = null;
    private int OpenPositionCount
    {
        get
        {
            return _openPositionCount;
        }
        set
        {
            if (value < 0) throw new Exception();
            _openPositionCount = value;
        }
    }
    private int _openPositionCount = 0;

    public float? LastTPPrice { get; private set; }

    public async Task Run(DateTime? terminationDate = null)
    {
        try
        {
            DateTime startDateTime = DateTime.UtcNow;

            Program.Logger.Information("Starting at {date}...", startDateTime);

            await Strategy.Initiate();

            await BingxUtilities.EnsureSuccessfulBingxResponse(await Trade.CloseOpenPositions());

            while (!Utilities.IsTerminationDatePassed(terminationDate))
            {
                if (!Utilities.HasTimeFrameReached(TimeFrame))
                {
                    await Utilities.Sleep(1000);
                    continue;
                }

                Program.Logger.Information("\n\n--- Tick ---");
                Program.Logger.Information("{tick}", DateTime.UtcNow);

                try { await BingxUtilities.NotifyListeners("Candle created."); }
                catch (NotificationException ex)
                {
                    Program.Logger.Error(ex, "Failure while notify users on candle creation.");
                    throw;
                }

                // 5 seconds delay to ensure the alert has reached the gmail's severs
                await Utilities.Sleep(7000);

                HttpResponseMessage? response = null;

                if (LastTPPrice is null && OpenPositionCount != 0 && await Strategy.CheckClosePositionSignal(IsLastOpenPositionLong, TimeFrame))
                {
                    ISignalProvider signalProvider = Strategy.GetLastSignal();
                    if (signalProvider.GetTPPrice() is null)
                    {
                        response = await Trade.CloseOpenPositions();
                        await BingxUtilities.EnsureSuccessfulBingxResponse(response);

                        OpenPositionCount = 0;

                        try { await BingxUtilities.CalculateFinancialPerformance(startDateTime, Trade); }
                        catch (System.Exception ex) { Program.Logger.Error(ex, "Failure while trying to calculate financial performance."); }
                    }
                }

                response = null;

                if (((LastTPPrice is not null && await Account.GetOpenPositionCount() == 0) || (LastTPPrice is null && OpenPositionCount == 0)) && await Strategy.CheckOpenPositionSignal(IsLastOpenPositionLong, TimeFrame))
                {
                    LastPrice = await Market.GetLastPrice(Trade.GetSymbol(), TimeFrame);

                    ISignalProvider signalProvider = Strategy.GetLastSignal();
                    LastTPPrice = signalProvider.GetTPPrice();

                    response = await Trade.SetLeverage(signalProvider.GetLeverage(), true);
                    await BingxUtilities.EnsureSuccessfulBingxResponse(response);
                    response = await Trade.SetLeverage(signalProvider.GetLeverage(), false);
                    await BingxUtilities.EnsureSuccessfulBingxResponse(response);

                    if (signalProvider.GetTPPrice() is null)
                        response = await Trade.OpenMarketOrder(signalProvider.IsSignalLong(), signalProvider.GetMargin() * (float)signalProvider.GetLeverage() / LastPrice, signalProvider.GetSLPrice());
                    else
                        response = await Trade.OpenMarketOrder(signalProvider.IsSignalLong(), signalProvider.GetMargin() * (float)signalProvider.GetLeverage() / LastPrice, (float)signalProvider.GetTPPrice()!, signalProvider.GetSLPrice());

                    await BingxUtilities.EnsureSuccessfulBingxResponse(response);

                    IsLastOpenPositionLong = signalProvider.IsSignalLong();
                    OpenPositionCount++;
                }
            }

            Program.Logger.Information("Ending at {date}...", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            Program.Logger.Fatal(ex, "Fatal exception has been thrown {message}", ex.Message);

            try { await BingxUtilities.NotifyListeners($"Fatal Exception has been thrown: {ex.GetType().Name}, Message: {ex.Message}"); }
            catch (Exception NotifyListenersException) { Program.Logger.Error(NotifyListenersException, "Failure while trying to send notification"); }
            try { await Trade.CloseOpenPositions(); }
            catch (Exception closeOpenPositionsEx)
            {
                Program.Logger.Error(closeOpenPositionsEx, "Failure while trying to close all of the open positions.");
                try { await BingxUtilities.NotifyListeners($"Fatal Exception has been thrown: {closeOpenPositionsEx.GetType().Name}, Message: {closeOpenPositionsEx.Message}"); }
                catch (Exception NotifyListenersException) { Program.Logger.Error(NotifyListenersException, "Failure while trying to send notification"); }
            }
            throw;
        }
        finally
        {
            try { await BingxUtilities.NotifyListeners("Bot terminated"); }
            catch (Exception NotifyListenersException) { Program.Logger.Error(NotifyListenersException, "Failure while trying to send notification"); }

            try { await Trade.CloseOpenPositions(); }
            catch (Exception closeOpenPositionsEx)
            {
                Program.Logger.Error(closeOpenPositionsEx, "Failure while trying to close all of the open positions.");
                try { await BingxUtilities.NotifyListeners($"Fatal Exception has been thrown: {closeOpenPositionsEx.GetType().Name}, Message: {closeOpenPositionsEx.Message}"); }
                catch (Exception NotifyListenersException) { Program.Logger.Error(NotifyListenersException, "Failure while trying to send notification"); }
            }

            Program.Logger.Information("Ended at {date}...", DateTime.UtcNow);
        }
    }
}
