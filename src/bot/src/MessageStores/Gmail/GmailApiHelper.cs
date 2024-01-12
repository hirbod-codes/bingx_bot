using System.Text;
using bot.src.MessageStores.Gmail.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace bot.src.MessageStores.Gmail;

public static class GmailApiHelper
{
    public static (GmailService service, string accessToken, string refreshToken) Authenticate(MessageProviderOptions emailProviderOptions)
    {
        new FileDataStore(emailProviderOptions.DataStoreFolderAddress).ClearAsync().Wait();

        Task<UserCredential> credentialTask = GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets() { ClientSecret = emailProviderOptions.ClientSecret, ClientId = emailProviderOptions.ClientId }, emailProviderOptions.Scopes, "user", CancellationToken.None, new FileDataStore(emailProviderOptions.DataStoreFolderAddress));
        credentialTask.Wait();
        UserCredential credential = credentialTask.Result;

        GmailService service = new(new BaseClientService.Initializer()
        {
            ApplicationName = "bot",
            HttpClientInitializer = credential,
            ValidateParameters = true
        });

        return (service, credential.Token.AccessToken.ToString(), credential.Token.RefreshToken.ToString());
    }

    public static string MsgNestedParts(IList<MessagePart> Parts)
    {
        if (Parts.Count <= 0)
            return string.Empty;

        string str = string.Empty;

        IList<MessagePart> PlainTestMail = Parts.Where(x => x.MimeType == "text/plain").ToList();
        IList<MessagePart> AttachmentMail = Parts.Where(x => x.MimeType == "multipart/alternative").ToList();

        if (PlainTestMail.Count > 0)
            foreach (MessagePart EachPart in PlainTestMail)
                if (EachPart.Parts != null)
                    return MsgNestedParts(EachPart.Parts);
                else if (EachPart.Body != null && EachPart.Body.Data != null)
                    str += EachPart.Body.Data;
        if (AttachmentMail.Count > 0)
            foreach (MessagePart EachPart in AttachmentMail)
                if (EachPart.Parts != null)
                    return MsgNestedParts(EachPart.Parts);
                else if (EachPart.Body != null && EachPart.Body.Data != null)
                    str += EachPart.Body.Data;

        return str;
    }

    public static string Base64Decode(string s)
    {
        string EncodedText;

        EncodedText = s.Replace("-", "+");
        EncodedText = EncodedText.Replace("_", "/");
        EncodedText = EncodedText.Replace(" ", "+");
        EncodedText = EncodedText.Replace("=", "+");

        // Fixed invalid length
        if (EncodedText.Length % 4 > 0)
            EncodedText += new string('=', 4 - EncodedText.Length % 4);
        else if (EncodedText.Length % 4 == 0)
        {
            EncodedText = EncodedText.Substring(0, EncodedText.Length - 1);
            if (EncodedText.Length % 4 > 0)
                EncodedText += new string('+', 4 - EncodedText.Length % 4);
        }

        //  Convert to Byte array
        byte[] ByteArray = Convert.FromBase64String(EncodedText);

        // Encoding to UTF8 Format
        return Encoding.UTF8.GetString(ByteArray);
    }

    public static byte[] Base64ToByte(string s)
    {
        // Replace all special Characters
        string EncodedText = s.Replace("-", "+");
        EncodedText = EncodedText.Replace("_", "/");
        EncodedText = EncodedText.Replace(" ", "+");
        EncodedText = EncodedText.Replace("=", "+");

        // Fixed invalid length
        if (EncodedText.Length % 4 > 0)
            EncodedText += new string('=', 4 - EncodedText.Length % 4);
        else if (EncodedText.Length % 4 == 0)
        {
            EncodedText = EncodedText.Substring(0, EncodedText.Length - 1);
            if (EncodedText.Length % 4 > 0)
                EncodedText += new string('+', 4 - EncodedText.Length % 4);
        }

        // Convert to Byte array
        return Convert.FromBase64String(EncodedText);
    }
}
