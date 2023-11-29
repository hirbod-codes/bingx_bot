using System.Net.Http.Headers;
using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;

namespace bingx_test;

public class UTBot
{
    public UTBot(Trade trade, Market market, int timeFrame, GmailApiHelper gmailAPIHelper, string ownerGmail, float margin, int leverage, float tpPercentage, float slPercentage)
    {
        Trade = trade;
        Market = market;
        TimeFrame = timeFrame;
        GmailApiHelper = gmailAPIHelper;
        OwnerGmail = ownerGmail;
        Margin = margin;
        Leverage = leverage;
        TpPercentage = tpPercentage;
        SlPercentage = slPercentage;
    }

    private string OwnerGmail { get; }
    public Trade Trade { get; }
    public Market Market { get; }
    public int TimeFrame { get; }
    public GmailApiHelper GmailApiHelper { get; }
    public string PreviousEmailSignalId { get; private set; } = string.Empty;
    public float Margin { get; }
    public int Leverage { get; }
    public float LastPrice { get; private set; }

    public float TpPercentage { get; } = 3.8f;
    public float SlPercentage { get; private set; } = 10f;

    public async Task Run()
    {
        System.Console.WriteLine();

        dynamic previousOrderId = 0;

        try
        {
            (await Trade.SetLeverage(Leverage, true)).EnsureSuccessStatusCode();
            (await Trade.SetLeverage(Leverage, false)).EnsureSuccessStatusCode();

            while (true)
            {
                if (DateTime.UtcNow.Minute % TimeFrame == 0 && DateTime.UtcNow.Second == 0)
                {
                    System.Console.WriteLine("--- Tick ---");
                    System.Console.WriteLine($"Minute ==> {DateTime.UtcNow.Minute}");

                    await Utilities.NotifyListeners();

                    bool? signal = null;
                    try { signal = CheckSignal(); }
                    catch (SignalCheckException)
                    {
                        if (previousOrderId != 0)
                        {
                            await Trade.CloseOrder(previousOrderId);
                            throw;
                        }
                    }

                    if (signal != null)
                    {
                        HttpResponseMessage response;

                        if (previousOrderId != 0)
                        {
                            await Trade.CloseOrder(previousOrderId);
                            previousOrderId = 0;
                        }

                        LastPrice = await Market.GetLastPrice(Trade.Symbol, TimeFrame);
                        System.Console.WriteLine($"last price => {LastPrice}");

                        bool isLong = (bool)signal;
                        response = await Trade.OpenMarketOrder(isLong, (float)(Margin * Leverage / LastPrice), Trade.GetTp(isLong, TpPercentage, LastPrice, Leverage), Trade.GetSl(isLong, SlPercentage, LastPrice, Leverage));

                        previousOrderId = (await Utilities.HandleResponse(response))["data"]!["order"]!["orderId"]!.GetValue<long>();
                    }
                }

                await Task.Delay(millisecondsDelay: 1000);
            }
        }
        catch (LeverageSetException)
        {
            throw;
        }
        catch (FormatException)
        {
            await Trade.CloseOrders();
            throw;
        }
        catch (LastPriceException)
        {
            if (previousOrderId != 0)
                await Trade.CloseOrder(previousOrderId);
            throw;
        }
        catch (OpenOrderException)
        {
            if (previousOrderId != 0)
                await Trade.CloseOrder(previousOrderId);
            throw;
        }
        catch (CloseOrderException)
        {
            await Trade.CloseOrders();
            throw;
        }
        catch (CloseOrdersException)
        {
            throw;
        }
    }

    public bool? CheckSignal()
    {
        System.Console.WriteLine("\n\nChecking for UT-bot signal...");

        List<Gmail> emails = GmailApiHelper.GetAllEmails(OwnerGmail, GmailApiHelper.SignalProviderEmail);
        if (!emails.Any())
        {
            System.Console.WriteLine("UT Bot signal has been checked, Result: No Signal(No Email Found)");
            return null;
        }

        Gmail mostRecentEmail = emails[0];

        if (mostRecentEmail.Id == PreviousEmailSignalId)
        {
            System.Console.WriteLine("UT Bot signal has been checked, Result: No Signal");
            return null;
        }
        else if (mostRecentEmail.Body.Contains("UT Long"))
        {
            System.Console.WriteLine("UT Bot signal has been checked, Result: Long Signal");
            PreviousEmailSignalId = mostRecentEmail.Id;
            return true;
        }
        else if (mostRecentEmail.Body.Contains("UT Short"))
        {
            System.Console.WriteLine("UT Bot signal has been checked, Result: Short Signal");
            PreviousEmailSignalId = mostRecentEmail.Id;
            return false;
        }
        else
            throw new SignalCheckException();
    }
}
