using Google.Apis.Gmail.v1;

namespace bot.src.MessageStores.Gmail.Models;

public class MessageStoreOptions : IMessageStoreOptions
{
    public string ClientId { get { return _clientId; } set { if (!string.IsNullOrEmpty(value)) _clientId = value; } }
    private string _clientId = null!;

    public string ClientSecret { get { return _clientSecret; } set { if (!string.IsNullOrEmpty(value)) _clientSecret = value; } }
    private string _clientSecret = null!;

    public string SignalProviderEmail { get { return _signalProviderEmail; } set { if (!string.IsNullOrEmpty(value)) _signalProviderEmail = value; } }
    private string _signalProviderEmail = null!;

    public string DataStoreFolderAddress { get { return _dataStoreFolderAddress; } set { if (!string.IsNullOrEmpty(value)) _dataStoreFolderAddress = value; } }
    private string _dataStoreFolderAddress = null!;

    public string OwnerGmail { get { return _ownerGmail; } set { if (!string.IsNullOrEmpty(value)) _ownerGmail = value; } }
    private string _ownerGmail = null!;

    public string[] Scopes { get; } = new string[] { GmailService.Scope.MailGoogleCom };
}
