using gmail_api;
using gmail_api.Models;

namespace strategies.src.UT;

public class UTSignals : ISignals
{
    public GmailApiHelper LongProvider { get; private set; }
    public string LongProviderGmail { get; private set; }
    public string LongSignalProvider { get; private set; }
    public string? LongProviderLastId { get; private set; } = null;

    public GmailApiHelper ShortProvider { get; private set; }
    public string ShortProviderGmail { get; private set; }
    public string ShortSignalProvider { get; private set; }
    public string? ShortProviderLastId { get; private set; } = null;

    public UTSignals(GmailProvider longProvider, GmailProvider shortProvider)
    {
        LongProvider = new(longProvider.ClientId, longProvider.ClientSecret, longProvider.Scopes, longProvider.SignalProviderEmail, longProvider.DataStoreFolderAddress);
        LongProviderGmail = longProvider.OwnerGmail;
        LongSignalProvider = longProvider.SignalProviderEmail;

        ShortProvider = new(shortProvider.ClientId, shortProvider.ClientSecret, shortProvider.Scopes, shortProvider.SignalProviderEmail, shortProvider.DataStoreFolderAddress);
        ShortProviderGmail = shortProvider.OwnerGmail;
        ShortSignalProvider = shortProvider.SignalProviderEmail;
    }

    public async Task Initiate()
    {
        await LongProvider.DeleteAllEmails(LongProviderGmail, LongSignalProvider);
        await ShortProvider.DeleteAllEmails(ShortProviderGmail, ShortSignalProvider);
    }

    public bool CheckLongSignal()
    {
        System.Console.WriteLine("\n\nChecking for UT-bot Long signal...");

        Gmail? mostRecentEmail = LongProvider.GetLastEmail(LongProviderGmail, LongSignalProvider);
        if (mostRecentEmail == null)
        {
            System.Console.WriteLine("UT Bot Long signal has been checked, Result: No Signal(No Email Found)");
            return false;
        }

        if (mostRecentEmail.Id == LongProviderLastId)
        {
            System.Console.WriteLine("UT Bot Long signal has been checked, Result: No Signal");
            return false;
        }
        else if (mostRecentEmail.Body.Contains("UT Long"))
        {
            System.Console.WriteLine("UT Bot Long signal has been checked, Result: Long Signal");
            LongProviderLastId = mostRecentEmail.Id;
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

        Gmail? mostRecentEmail = ShortProvider.GetLastEmail(ShortProviderGmail, ShortSignalProvider);
        if (mostRecentEmail == null)
        {
            System.Console.WriteLine("UT Bot Short signal has been checked, Result: No Signal(No Email Found)");
            return false;
        }

        if (mostRecentEmail.Id == ShortProviderLastId)
        {
            System.Console.WriteLine("UT Bot Short signal has been checked, Result: No Signal");
            return false;
        }
        else if (mostRecentEmail.Body.Contains("UT Short"))
        {
            System.Console.WriteLine("UT Bot Short signal has been checked, Result: Short Signal");
            ShortProviderLastId = mostRecentEmail.Id;
            return true;
        }
        else
        {
            System.Console.WriteLine("UT Bot Short signal has been checked, Result: No Signal");
            return false;
        }
    }
}
