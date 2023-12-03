using gmail_api;
using gmail_api.Models;

namespace strategies.src.EMA;

public class EMASignals : ISignals
{
    public GmailApiHelper LongProvider { get; private set; }
    public string LongProviderGmail { get; private set; }
    public string LongSignalProvider { get; private set; }
    public string? LongProviderLastId { get; private set; } = null;

    public GmailApiHelper ShortProvider { get; private set; }
    public string ShortProviderGmail { get; private set; }
    public string ShortSignalProvider { get; private set; }
    public string? ShortProviderLastId { get; private set; } = null;

    public EMASignals(GmailProvider longGmailProvider, GmailProvider shortGmailProvider)
    {
        LongProvider = new(longGmailProvider.ClientId, longGmailProvider.ClientSecret, longGmailProvider.Scopes, longGmailProvider.SignalProviderEmail, longGmailProvider.DataStoreFolderAddress);
        LongProviderGmail = longGmailProvider.SignalProviderEmail;
        LongSignalProvider = longGmailProvider.SignalProviderEmail;

        ShortProvider = new(shortGmailProvider.ClientId, shortGmailProvider.ClientSecret, shortGmailProvider.Scopes, shortGmailProvider.SignalProviderEmail, shortGmailProvider.DataStoreFolderAddress);
        ShortProviderGmail = shortGmailProvider.SignalProviderEmail;
        ShortSignalProvider = shortGmailProvider.SignalProviderEmail;
    }

    public async Task Initiate()
    {
        await LongProvider.DeleteAllEmails(LongProviderGmail, LongSignalProvider);
        await ShortProvider.DeleteAllEmails(ShortProviderGmail, ShortSignalProvider);
    }

    public bool CheckLongSignal()
    {
        System.Console.WriteLine("\n\nChecking for EMA cross-up signal...");

        Gmail? mostRecentEmail = LongProvider.GetLastEmail(LongProviderGmail, LongProvider.SignalProviderEmail);
        if (mostRecentEmail == null)
        {
            System.Console.WriteLine("EMA cross-up signal has been checked, Result: No Signal(No Email Found)");
            return false;
        }

        if (mostRecentEmail.Id == LongProviderLastId)
        {
            System.Console.WriteLine("EMA cross-up signal has been checked, Result: No Signal");
            return false;
        }
        else if (mostRecentEmail.Body.Contains("Crossing Up"))
        {
            System.Console.WriteLine("EMA cross-up signal has been checked, Result: Long Signal");
            return true;
        }
        else
        {
            System.Console.WriteLine("EMA cross-up signal has been checked, Result: No Signal");
            return false;
        }
    }

    public bool CheckShortSignal()
    {
        System.Console.WriteLine("\n\nChecking for EMA cross-down signal...");

        Gmail? mostRecentEmail = ShortProvider.GetLastEmail(ShortProviderGmail, ShortProvider.SignalProviderEmail);
        if (mostRecentEmail == null)
        {
            System.Console.WriteLine("EMA cross-down signal has been checked, Result: No Signal(No Email Found)");
            return false;
        }

        if (mostRecentEmail.Id == ShortProviderLastId)
        {
            System.Console.WriteLine("EMA cross-down signal has been checked, Result: No Signal");
            return false;
        }
        else if (mostRecentEmail.Body.Contains("Crossing Down"))
        {
            System.Console.WriteLine("EMA cross-down signal has been checked, Result: Short Signal");
            return true;
        }
        else
        {
            System.Console.WriteLine("EMA cross-down signal has been checked, Result: No Signal");
            return false;
        }
    }
}
