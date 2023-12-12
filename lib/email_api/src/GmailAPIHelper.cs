using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using email_api.Models;

namespace email_api;

public static class GmailApiHelper
{
    public static (GmailService service, string accessToken, string refreshToken) Authenticate(EmailProviderOptions emailProviderOptions)
    {
        new FileDataStore(emailProviderOptions.DataStoreFolderAddress).ClearAsync().Wait();

        Task<UserCredential> credentialTask = GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets() { ClientSecret = emailProviderOptions.ClientSecret, ClientId = emailProviderOptions.ClientId }, emailProviderOptions.Scopes, "user", CancellationToken.None, new FileDataStore(emailProviderOptions.DataStoreFolderAddress));
        credentialTask.Wait();
        UserCredential credential = credentialTask.Result;

        GmailService service = new(new BaseClientService.Initializer()
        {
            ApplicationName = "UT-bot",
            HttpClientInitializer = credential,
            ValidateParameters = true
        });

        return (service, credential.Token.AccessToken.ToString(), credential.Token.RefreshToken.ToString());
    }

    public static string MsgNestedParts(IList<MessagePart> Parts)
    {
        string str = string.Empty;
        if (Parts.Count < 0)
            return string.Empty;
        else
        {
            IList<MessagePart> PlainTestMail = Parts.Where(x => x.MimeType == "text/plain").ToList();
            IList<MessagePart> AttachmentMail = Parts.Where(x => x.MimeType == "multipart/alternative").ToList();

            if (PlainTestMail.Count > 0)
                foreach (MessagePart EachPart in PlainTestMail)
                    if (EachPart.Parts == null)
                    {
                        if (EachPart.Body != null && EachPart.Body.Data != null)
                            str += EachPart.Body.Data;
                    }
                    else
                        return MsgNestedParts(EachPart.Parts);
            if (AttachmentMail.Count > 0)
                foreach (MessagePart EachPart in AttachmentMail)
                    if (EachPart.Parts == null)
                    {
                        if (EachPart.Body != null && EachPart.Body.Data != null)
                            str += EachPart.Body.Data;
                    }
                    else
                        return MsgNestedParts(EachPart.Parts);
            return str;
        }
    }

    public static string Base64Decode(string Base64Test)
    {
        string EncodedText;

        //STEP-1: Replace all special Character of Base64Test
        EncodedText = Base64Test.Replace("-", "+");
        EncodedText = EncodedText.Replace("_", "/");
        EncodedText = EncodedText.Replace(" ", "+");
        EncodedText = EncodedText.Replace("=", "+");

        //STEP-2: Fixed invalid length of Base64Test
        if (EncodedText.Length % 4 > 0)
            EncodedText += new string('=', 4 - EncodedText.Length % 4);
        else if (EncodedText.Length % 4 == 0)
        {
            EncodedText = EncodedText.Substring(0, EncodedText.Length - 1);
            if (EncodedText.Length % 4 > 0)
                EncodedText += new string('+', 4 - EncodedText.Length % 4);
        }

        //STEP-3: Convert to Byte array
        byte[] ByteArray = Convert.FromBase64String(EncodedText);

        //STEP-4: Encoding to UTF8 Format
        return Encoding.UTF8.GetString(ByteArray);
    }

    public static byte[] Base64ToByte(string Base64Test)
    {
        string EncodedText = string.Empty;
        //STEP-1: Replace all special Character of Base64Test
        EncodedText = Base64Test.Replace("-", "+");
        EncodedText = EncodedText.Replace("_", "/");
        EncodedText = EncodedText.Replace(" ", "+");
        EncodedText = EncodedText.Replace("=", "+");

        //STEP-2: Fixed invalid length of Base64Test
        if (EncodedText.Length % 4 > 0)
            EncodedText += new string('=', 4 - EncodedText.Length % 4);
        else if (EncodedText.Length % 4 == 0)
        {
            EncodedText = EncodedText.Substring(0, EncodedText.Length - 1);
            if (EncodedText.Length % 4 > 0)
                EncodedText += new string('+', 4 - EncodedText.Length % 4);
        }

        //STEP-3: Convert to Byte array
        return Convert.FromBase64String(EncodedText);
    }
}
