using gmail_api;
using gmail_api.Models;
using Serilog.Core;

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
    private Logger Logger { get; }

    public UTSignals(GmailProvider longProvider, GmailProvider shortProvider, Logger logger)
    {
        LongProvider = new(longProvider.ClientId, longProvider.ClientSecret, longProvider.Scopes, longProvider.SignalProviderEmail, longProvider.DataStoreFolderAddress);
        LongProviderGmail = longProvider.OwnerGmail;
        LongSignalProvider = longProvider.SignalProviderEmail;

        ShortProvider = new(shortProvider.ClientId, shortProvider.ClientSecret, shortProvider.Scopes, shortProvider.SignalProviderEmail, shortProvider.DataStoreFolderAddress);
        ShortProviderGmail = shortProvider.OwnerGmail;
        ShortSignalProvider = shortProvider.SignalProviderEmail;
        Logger = logger;
    }

    public async Task Initiate()
    {
        await LongProvider.DeleteAllEmails(LongProviderGmail, LongSignalProvider);
        await ShortProvider.DeleteAllEmails(ShortProviderGmail, ShortSignalProvider);
    }

    public bool CheckLongSignal()
    {
        Logger.Information("Checking for UT-bot Long signal...");

        Gmail? mostRecentEmail = LongProvider.GetLastEmail(LongProviderGmail, LongSignalProvider);
        if (mostRecentEmail == null)
        {
            Logger.Information("UT Bot Long signal has been checked, Result: No Signal(No Email Found)");
            return false;
        }

        if (mostRecentEmail.Id == LongProviderLastId)
        {
            Logger.Information("UT Bot Long signal has been checked, Result: No Signal");
            return false;
        }
        else if (mostRecentEmail.Body.Contains("UT Long"))
        {
            Logger.Information("UT Bot Long signal has been checked, Result: Long Signal");
            LongProviderLastId = mostRecentEmail.Id;
            return true;
        }
        else
        {
            Logger.Information("UT Bot Long signal has been checked, Result: No Signal");
            return false;
        }
    }

    public bool CheckShortSignal()
    {
        Logger.Information("Checking for UT-bot Short signal...");

        Gmail? mostRecentEmail = ShortProvider.GetLastEmail(ShortProviderGmail, ShortSignalProvider);
        if (mostRecentEmail == null)
        {
            Logger.Information("UT Bot Short signal has been checked, Result: No Signal(No Email Found)");
            return false;
        }

        if (mostRecentEmail.Id == ShortProviderLastId)
        {
            Logger.Information("UT Bot Short signal has been checked, Result: No Signal");
            return false;
        }
        else if (mostRecentEmail.Body.Contains("UT Short"))
        {
            Logger.Information("UT Bot Short signal has been checked, Result: Short Signal");
            ShortProviderLastId = mostRecentEmail.Id;
            return true;
        }
        else
        {
            Logger.Information("UT Bot Short signal has been checked, Result: No Signal");
            return false;
        }
    }
}
