using email_api.Models;
using email_api.src;
using Serilog;

namespace strategies.src;

public class GeneralSignals : ISignalProvider
{
    public const string EMAIL_SEPARATOR = "%%%68411864%%%";
    public const string MESSAGE_DELIMITER = ",";

    private IEmailProvider EmailProvider { get; }
    private ILogger Logger { get; }
    private string? LastEmailId { get; set; }
    private DateTime SignalTime { get; set; }
    private int Leverage { get; set; }
    private float Margin { get; set; }
    private bool LongSignal { get; set; }
    private float SLPrice { get; set; }
    private float? TPPrice { get; set; }

    public GeneralSignals(IEmailProvider emailProvider, ILogger logger)
    {
        EmailProvider = emailProvider;
        Logger = logger;
    }

    public async Task<bool> CheckSignals()
    {
        Logger.Information("Checking for signal...");

        Email? lastEmail = await EmailProvider.GetLastEmail(EmailProvider.GetSignalProviderEmail());
        if (lastEmail == null)
        {
            Logger.Information("Signal has been checked, Result: No Signal(No Email Found)");
            return false;
        }

        CollectPropertiesFromEmail(lastEmail);

        Logger.Information("Signal has been checked, Result: true");
        return true;
    }

    /// <summary>
    /// This method expects an email body with a MESSAGE_DELIMITER delimited list of key value pairs with following keys: side,time,margin,leverage,tp(optional),sl
    /// </summary>
    private void CollectPropertiesFromEmail(Email email)
    {
        Logger.Information("Collecting properties from email...");
        string[] propertiesPairs = email.Body.Split(EMAIL_SEPARATOR, StringSplitOptions.TrimEntries)[1].Split(MESSAGE_DELIMITER);
        Dictionary<string, string> properties = new();
        for (int i = 0; i < propertiesPairs.Length; i++)
        {
            string[] keyValue = propertiesPairs[i].Split("=", StringSplitOptions.RemoveEmptyEntries);
            if (keyValue.Length != 2)
                throw new Exception("Invalid Email Message");
            properties.Add(propertiesPairs[i].Split("=")[0], propertiesPairs[i].Split("=")[1]);
        }

        SignalTime = DateTime.Parse(properties["time"]);
        Logger.Information("SignalTime: {SignalTime}", SignalTime);

        LongSignal = properties["side"].ToLower() == "long" || properties["side"].ToLower() == "1";
        Logger.Information("LongSignal: {LongSignal}", LongSignal);

        Margin = float.Parse(properties["margin"]);
        Logger.Information("Margin: {margin}", Margin);

        Leverage = (int)float.Parse(properties["leverage"]);
        Logger.Information("Leverage: {leverage}", Leverage);

        properties.TryGetValue("tp", out string? tp);
        TPPrice = tp is not null ? float.Parse(tp) : null;
        Logger.Information("TPPrice: {tpPrice}", TPPrice);

        SLPrice = float.Parse(properties["sl"]);
        Logger.Information("SLPrice: {slPrice}", SLPrice);

        Logger.Information("Finished Collecting properties from email...");
    }

    public int GetLeverage() => Leverage;

    public float GetMargin() => Margin;

    public float GetSLPrice() => SLPrice;

    public float? GetTPPrice() => TPPrice;

    public async Task Initiate() => await EmailProvider.DeleteEmails();

    public bool IsSignalLong() => LongSignal;

    public void ResetSignals()
    {
        SignalTime = DateTime.UtcNow.AddDays(-10);
        Leverage = 0;
        Margin = 0;
        LongSignal = false;
        SLPrice = 0;
        TPPrice = null;
    }

    public DateTime GetSignalTime() => SignalTime;
}
