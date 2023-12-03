using System.Net.Http.Json;
using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using gmail_api.Models;
using gmail_api.Exceptions;

namespace gmail_api;

public class GmailApiHelper
{
    public GmailService Service { get; private set; }
    public string[] Scopes { get; } = Array.Empty<string>();
    public string ClientSecret { get; } = null!;
    public string ClientId { get; } = null!;
    public string SignalProviderEmail { get; }
    public string DataStoreFolderAddress { get; }
    public string AccessToken { get; private set; } = null!;
    public string RefreshToken { get; private set; } = null!;

    public GmailApiHelper(string clientId, string clientSecret, string[] scopes, string signalProviderEmail, string dataStoreFolderAddress)
    {
        ClientId = clientId;
        ClientSecret = clientSecret;
        Scopes = scopes;
        SignalProviderEmail = signalProviderEmail;
        DataStoreFolderAddress = dataStoreFolderAddress;
        Service = GetService();
    }

    public GmailService GetService()
    {
        new FileDataStore(DataStoreFolderAddress).ClearAsync().Wait();

        Task<UserCredential> credentialTask = GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets() { ClientSecret = ClientSecret, ClientId = ClientId }, Scopes, "user", CancellationToken.None, new FileDataStore(DataStoreFolderAddress));
        credentialTask.Wait();
        UserCredential credential = credentialTask.Result;

        AccessToken = credential.Token.AccessToken.ToString();
        RefreshToken = credential.Token.RefreshToken.ToString();

        GmailService service = new(new BaseClientService.Initializer()
        {
            ApplicationName = "UT-bot",
            HttpClientInitializer = credential,
            ValidateParameters = true
        });

        return service;
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

    public void MsgMarkAsRead(string ownerGmail, string MsgId)
    {
        //MESSAGE MARKS AS READ AFTER READING MESSAGE
        ModifyMessageRequest mods = new()
        {
            AddLabelIds = null,
            RemoveLabelIds = new List<string> { "UNREAD" }
        };

        Service.Users.Messages.Modify(mods, ownerGmail, MsgId).Execute();
    }

    public async Task DeleteAllEmails(string ownerGmail, string? filterByEmail = null)
    {
        List<Gmail> emails = GetAllEmails(ownerGmail, filterByEmail);

        System.Console.WriteLine("\n\nInitial deletion of emails...");

        List<string> Ids = emails.ConvertAll(x => x.Id).ToList();

        if (!Ids.Any())
        {
            System.Console.WriteLine("There is no email to delete...");
            return;
        }

        // string response = await Service.Users.Messages.BatchDelete(new BatchDeleteMessagesRequest() { Ids = Ids }, ownerGmail).ExecuteAsync();

        HttpClient httpClient = new();
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + AccessToken);

        ownerGmail = ownerGmail.Replace("@", "%40");
        HttpResponseMessage response = await httpClient.PostAsync($"https://gmail.googleapis.com/gmail/v1/users/{ownerGmail}/messages/batchDelete?access_token=" + AccessToken, JsonContent.Create(new { Ids = Ids }));

        var t = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode) throw new EmailsDeletionException();

        System.Console.WriteLine("Deletion of emails successfully finished...");
    }

    public List<Gmail> GetAllEmails(string ownerGmail, string? filterByEmail = null)
    {
        System.Console.WriteLine("\n\nGetting all the emails...");
        System.Console.WriteLine($"filterByEmail value is: {filterByEmail}");

        List<Gmail> EmailList = new();
        UsersResource.MessagesResource.ListRequest ListRequest = Service.Users.Messages.List(ownerGmail);
        ListRequest.LabelIds = "INBOX";
        ListRequest.IncludeSpamTrash = false;
        if (filterByEmail != null)
            ListRequest.Q = $"from:{filterByEmail}";

        //GET ALL EMAILS
        ListMessagesResponse ListResponse = ListRequest.Execute();

        if (ListResponse == null || ListResponse.Messages == null)
        {
            System.Console.WriteLine("Finished getting all the emails...");
            return EmailList;
        }

        //LOOP THROUGH EACH EMAIL AND GET WHAT FIELDS I WANT
        foreach (Message message in ListResponse.Messages)
        {
            //MESSAGE MARKS AS READ AFTER READING MESSAGE
            MsgMarkAsRead(ownerGmail, message.Id);

            UsersResource.MessagesResource.GetRequest Message = Service.Users.Messages.Get(ownerGmail, message.Id);
            Console.WriteLine("\n-----------------NEW MAIL----------------------");
            Console.WriteLine("STEP-1: Message ID:" + message.Id);

            //MAKE ANOTHER REQUEST FOR THAT EMAIL ID...
            Message MsgContent = Message.Execute();

            if (MsgContent == null)
                continue;

            string FromAddress = string.Empty;
            string Date = string.Empty;
            string Subject;
            string MailBody;
            string ReadableText;

            //LOOP THROUGH THE HEADERS AND GET THE FIELDS WE NEED (SUBJECT, MAIL)
            foreach (var MessageParts in MsgContent.Payload.Headers)
                if (MessageParts.Name == "From")
                    FromAddress = MessageParts.Value;
                else if (MessageParts.Name == "Date")
                    Date = MessageParts.Value;
                else if (MessageParts.Name == "Subject")
                    Subject = MessageParts.Value;

            //READ MAIL BODY-------------------------------------------------------------------------------------
            Console.WriteLine("STEP-2: Read Mail Body");

            if (MsgContent.Payload.Parts == null && MsgContent.Payload.Body != null)
                MailBody = MsgContent.Payload.Body.Data;
            else
                MailBody = MsgNestedParts(MsgContent.Payload.Parts ?? throw new NullReferenceException("Failed to set mail's body."));

            //BASE64 TO READABLE TEXT--------------------------------------------------------------------------------
            ReadableText = Base64Decode(MailBody);

            Console.WriteLine("STEP-4: Identifying & Configure Mails.");

            if (!string.IsNullOrEmpty(ReadableText))
            {
                Gmail Gmail = new()
                {
                    From = FromAddress,
                    Body = ReadableText,
                    Id = message.Id,
                    To = ownerGmail,
                    ETag = message.ETag
                };
                if (DateTime.TryParse(Date, out DateTime dt))
                    Gmail.MailDateTime = dt;
                EmailList.Add(Gmail);
            }
        }

        System.Console.WriteLine("Finished getting all the emails...");
        return EmailList;
    }

    public Gmail? GetLastEmail(string ownerGmail, string? filterByEmail = null)
    {
        System.Console.WriteLine("\n\nGetting the last email...");
        System.Console.WriteLine($"filterByEmail value is: {filterByEmail}");

        Gmail? lastEmail = null;
        UsersResource.MessagesResource.ListRequest ListRequest = Service.Users.Messages.List(ownerGmail);
        ListRequest.LabelIds = "INBOX";
        ListRequest.IncludeSpamTrash = false;
        if (filterByEmail != null)
            ListRequest.Q = $"from:{filterByEmail}";

        ListMessagesResponse ListResponse = ListRequest.Execute();

        if (ListResponse == null || ListResponse.Messages == null || !ListResponse.Messages.Any())
        {
            System.Console.WriteLine("No email found...");
            System.Console.WriteLine("Finished getting the last email...");
            return null;
        }

        System.Console.WriteLine($"{ListResponse.Messages.Count} emails has received...");
        foreach (Message message in ListResponse.Messages)
        {
            //MESSAGE MARKS AS READ AFTER READING MESSAGE
            MsgMarkAsRead(ownerGmail, message.Id);

            UsersResource.MessagesResource.GetRequest Message = Service.Users.Messages.Get(ownerGmail, message.Id);
            Console.WriteLine("\n-----------------NEW MAIL----------------------");
            Console.WriteLine("STEP-1: Message ID:" + message.Id);

            //MAKE ANOTHER REQUEST FOR THAT EMAIL ID...
            Message MsgContent = Message.Execute();

            if (MsgContent == null)
                continue;

            string FromAddress = string.Empty;
            string Date = string.Empty;
            string Subject;
            string MailBody;
            string ReadableText;

            //LOOP THROUGH THE HEADERS AND GET THE FIELDS WE NEED (SUBJECT, MAIL)
            foreach (var MessageParts in MsgContent.Payload.Headers)
                if (MessageParts.Name == "From")
                    FromAddress = MessageParts.Value;
                else if (MessageParts.Name == "Date")
                    Date = MessageParts.Value;
                else if (MessageParts.Name == "Subject")
                    Subject = MessageParts.Value;

            //READ MAIL BODY-------------------------------------------------------------------------------------
            Console.WriteLine("STEP-2: Read Mail Body");

            if (MsgContent.Payload.Parts == null && MsgContent.Payload.Body != null)
                MailBody = MsgContent.Payload.Body.Data;
            else
                MailBody = MsgNestedParts(MsgContent.Payload.Parts ?? throw new NullReferenceException("Failed to set mail's body."));

            //BASE64 TO READABLE TEXT--------------------------------------------------------------------------------
            ReadableText = Base64Decode(MailBody);

            Console.WriteLine("STEP-4: Identifying & Configure Mails.");

            if (!string.IsNullOrEmpty(ReadableText))
            {
                lastEmail = new()
                {
                    From = FromAddress,
                    Body = ReadableText,
                    Id = message.Id,
                    To = ownerGmail,
                    ETag = message.ETag
                };

                if (DateTime.TryParse(Date, out DateTime dt))
                    lastEmail.MailDateTime = dt;

                break;
            }
        }

        System.Console.WriteLine("Finished getting the last email...");
        return lastEmail;
    }
}
