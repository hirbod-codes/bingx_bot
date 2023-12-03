using Google.Apis.Gmail.v1;
using Microsoft.Extensions.Configuration;
using bingx_ut_bot.Exceptions;
using bingx_api;
using gmail_api;
using bingx_api.Exceptions;
using gmail_api.Models;
using gmail_api.Exceptions;

namespace bingx_ut_bot;

public class Bot
{
    public Bot(IConfigurationSection bingxApi, IConfigurationSection gmailApi)
    {
        BingxApi = bingxApi;
        GmailApi = gmailApi;

        Trade = new Trade(bingxApi["BaseUrl"]!, bingxApi["ApiKey"]!, bingxApi["ApiSecret"]!, bingxApi["Symbol"]!);
        Market = new Market(bingxApi["BaseUrl"]!, bingxApi["ApiKey"]!, bingxApi["ApiSecret"]!, bingxApi["Symbol"]!);
        TimeFrame = int.Parse(bingxApi["TimeFrame"]!);
        Margin = float.Parse(bingxApi["Margin"]!);
        Leverage = int.Parse(bingxApi["Leverage"]!);
        TpPercentage = !string.IsNullOrEmpty(bingxApi["TpPercentage"]) ? float.Parse(bingxApi["TpPercentage"]!) : null;
        SlPercentage = float.Parse(bingxApi["SlPercentage"]!);

        GmailApiHelperForLong = new(gmailApi["LongProvider:ClientId"]!, gmailApi["LongProvider:ClientSecret"]!, new string[] { GmailService.Scope.MailGoogleCom }, gmailApi["LongProvider:SignalProviderEmail"]!, gmailApi["LongProvider:DataStoreFolderAddress"]!);
        LongOwnerGmail = gmailApi["LongProvider:Gmail"]!;
        GmailApiHelperForShort = new(gmailApi["ShortProvider:ClientId"]!, gmailApi["ShortProvider:ClientSecret"]!, new string[] { GmailService.Scope.MailGoogleCom }, gmailApi["ShortProvider:SignalProviderEmail"]!, gmailApi["LongProvider:DataStoreFolderAddress"]!);
        ShortOwnerGmail = gmailApi["ShortProvider:Gmail"]!;
    }

    public IConfigurationSection BingxApi { get; }
    public IConfigurationSection GmailApi { get; }

    public Trade Trade { get; }
    public Market Market { get; }
    public int TimeFrame { get; }
    public float Margin { get; }
    public int Leverage { get; }
    public float LastPrice { get; private set; }
    public float? TpPercentage { get; } = null;
    public float SlPercentage { get; private set; } = 10f;

    public string PreviousEmailLongSignalId { get; private set; } = string.Empty;
    public string PreviousEmailShortSignalId { get; private set; } = string.Empty;

    public GmailApiHelper GmailApiHelperForLong { get; }
    private string LongOwnerGmail { get; }
    public GmailApiHelper GmailApiHelperForShort { get; }
    private string ShortOwnerGmail { get; }

    private long CurrentOpenOrderId { get; set; }
    private bool? IsCurrentOpenPositionLong { get; set; } = null;

    public async Task Run()
    {
        System.Console.WriteLine("Starting...");

        await GmailApiHelperForLong.DeleteAllEmails(GmailApi["LongProvider:Gmail"]!, GmailApi["LongProvider:SignalProviderEmail"]!);
        await GmailApiHelperForShort.DeleteAllEmails(GmailApi["ShortProvider:Gmail"]!, GmailApi["ShortProvider:SignalProviderEmail"]!);

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

                    await Utilities.NotifyListeners("Candle Created.");

                    // 2 seconds delay to ensure the alert has reached gmail's severs
                    await Task.Delay(millisecondsDelay: 3000);

                    HttpResponseMessage response;

                    bool? signal = null;
                    try { signal = CheckSignal(); }
                    catch (SignalCheckException)
                    {
                        if (CurrentOpenOrderId != 0)
                        {
                            response = await Trade.CloseOpenPositions();
                            await Utilities.HandleBingxResponse(response);
                            throw;
                        }
                    }

                    if (signal != null)
                    {
                        response = await Trade.CloseOpenPositions();
                        await Utilities.HandleBingxResponse(response);
                        CurrentOpenOrderId = 0;

                        LastPrice = await Market.GetLastPrice(Trade.Symbol, TimeFrame);

                        bool isLong = (bool)signal;
                        response = await Trade.OpenMarketOrder(isLong, (float)(Margin * Leverage / LastPrice), TpPercentage != null ? Trade.CalculateTp(isLong, (float)TpPercentage, LastPrice, Leverage) : null, Trade.CalculateSl(isLong, SlPercentage, LastPrice, Leverage));
                        response.EnsureSuccessStatusCode();

                        CurrentOpenOrderId = (await Utilities.HandleBingxResponse(response))["data"]!["order"]!["orderId"]!.GetValue<long>();
                        IsCurrentOpenPositionLong = signal;
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
    public bool? CheckSignal() => IsCurrentOpenPositionLong switch
    {
        null => CheckLongSignal() ? true : (CheckShortSignal() ? false : null),
        false => CheckLongSignal() ? true : null,
        true => CheckShortSignal() ? false : null
    };

    public bool CheckLongSignal()
    {
        System.Console.WriteLine("\n\nChecking for UT-bot Long signal...");

        Gmail? mostRecentEmail = GmailApiHelperForLong.GetLastEmail(LongOwnerGmail, GmailApiHelperForLong.SignalProviderEmail);
        if (mostRecentEmail == null)
        {
            System.Console.WriteLine("UT Bot Long signal has been checked, Result: No Signal(No Email Found)");
            return false;
        }

        if (mostRecentEmail.Id == PreviousEmailLongSignalId)
        {
            System.Console.WriteLine("UT Bot Long signal has been checked, Result: No Signal");
            return false;
        }
        else if (mostRecentEmail.Body.Contains("UT Long"))
        {
            System.Console.WriteLine("UT Bot Long signal has been checked, Result: Long Signal");
            PreviousEmailLongSignalId = mostRecentEmail.Id;
            return true;
        }
        else
        {
            System.Console.WriteLine("UT Bot Long signal has been checked, Result: No Signal");
            return false;
        }
    }

    public bool CheckShortSignal()
    {
        System.Console.WriteLine("\n\nChecking for UT-bot Short signal...");

        Gmail? mostRecentEmail = GmailApiHelperForShort.GetLastEmail(ShortOwnerGmail, GmailApiHelperForShort.SignalProviderEmail);
        if (mostRecentEmail == null)
        {
            System.Console.WriteLine("UT Bot Short signal has been checked, Result: No Signal(No Email Found)");
            return false;
        }

        if (mostRecentEmail.Id == PreviousEmailShortSignalId)
        {
            System.Console.WriteLine("UT Bot Short signal has been checked, Result: No Signal");
            return false;
        }
        else if (mostRecentEmail.Body.Contains("UT Short"))
        {
            System.Console.WriteLine("UT Bot Short signal has been checked, Result: Short Signal");
            PreviousEmailShortSignalId = mostRecentEmail.Id;
            return true;
        }
        else
        {
            System.Console.WriteLine("UT Bot Short signal has been checked, Result: No Signal");
            return false;
        }
    }
}
