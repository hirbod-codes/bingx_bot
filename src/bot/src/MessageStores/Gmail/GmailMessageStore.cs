using System.Text.Json;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Serilog;
using Message = bot.src.MessageStores.Gmail.Models.Message;
using GmailMessage = Google.Apis.Gmail.v1.Data.Message;
using bot.src.MessageStores.Gmail.Models;

namespace bot.src.MessageStores.Gmail;

public class GmailMessageStore : IMessageStore
{
    public GmailService Service { get; private set; }
    public MessageProviderOptions EmailProviderOptions { get; }
    public string AccessToken { get; private set; } = null!;
    public string RefreshToken { get; private set; } = null!;

    private ILogger Logger { get; }


    public GmailMessageStore(MessageProviderOptions messageProviderOptions, ILogger logger)
    {
        EmailProviderOptions = messageProviderOptions;
        (GmailService service, string accessToken, string refreshToken) = GmailApiHelper.Authenticate(messageProviderOptions);
        Service = service;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        Logger = logger.ForContext<GmailMessageStore>();
    }

    public string GetSignalProviderEmail() => EmailProviderOptions.SignalProviderEmail;

    public string GetProviderEmail() => EmailProviderOptions.OwnerGmail;

    public async Task<IMessage?> GetLastMessage()
    {
        Logger.Information("Getting the last message...");
        Logger.Information("ownerGmail value is: {ownerGmail}", EmailProviderOptions.OwnerGmail);

        UsersResource.MessagesResource.ListRequest listRequest = Service.Users.Messages.List(EmailProviderOptions.OwnerGmail);
        listRequest.LabelIds = "INBOX";
        listRequest.IncludeSpamTrash = true;

        IMessage? message = await GetLastMessage(listRequest);

        Logger.Information("Finished getting the last message... {@message}", JsonSerializer.Serialize(message));
        return message;
    }

    public async Task<IMessage?> GetLastMessage(string from)
    {
        Logger.Information("Getting the last message...");
        Logger.Information("ownerGmail value is: {ownerGmail}", EmailProviderOptions.OwnerGmail);
        Logger.Information("from value is: {from}", from);

        UsersResource.MessagesResource.ListRequest listRequest = Service.Users.Messages.List(EmailProviderOptions.OwnerGmail);
        listRequest.LabelIds = "INBOX";
        listRequest.IncludeSpamTrash = true;
        listRequest.Q = $"from:{from}";

        IMessage? message = await GetLastMessage(listRequest);

        Logger.Information("Finished getting the last message... {@message}", JsonSerializer.Serialize(message));
        return message;
    }

    private async Task<IMessage?> GetLastMessage(UsersResource.MessagesResource.ListRequest listRequest)
    {
        ListMessagesResponse listResponse = await listRequest.ExecuteAsync();

        if (listResponse == null || listResponse.Messages == null || !listResponse.Messages.Any())
            return null;

        foreach (GmailMessage gmailMessage in listResponse.Messages)
        {
            GmailMessage fetchedGmailMessage = await Service.Users.Messages.Get(EmailProviderOptions.OwnerGmail, gmailMessage.Id).ExecuteAsync();
            IMessage? message = ProcessMessage(fetchedGmailMessage);
            if (message is null)
                continue;

            return message;
        }

        return null;
    }

    public async Task<IMessage?> GetMessage(string id)
    {
        Logger.Information("Getting the message...");
        Logger.Information("ownerGmail value is: {ownerGmail}", EmailProviderOptions.OwnerGmail);

        UsersResource.MessagesResource.GetRequest getRequest = Service.Users.Messages.Get(EmailProviderOptions.OwnerGmail, id);
        GmailMessage gmailMessage = await getRequest.ExecuteAsync();
        IMessage? message = ProcessMessage(gmailMessage);

        Logger.Information("Found the message: {@message}", JsonSerializer.Serialize(message));
        Logger.Information("Finished Getting the message...");
        return message;
    }

    public async Task<IMessage?> GetMessage(string attribute, string value)
    {
        Logger.Information("Getting the message with attributes...");
        Logger.Information("ownerGmail value is: {ownerGmail}", EmailProviderOptions.OwnerGmail);
        Logger.Information("attribute is: {attribute}", attribute);
        Logger.Information("value is: {value}", value);

        UsersResource.MessagesResource.ListRequest listRequest = Service.Users.Messages.List(EmailProviderOptions.OwnerGmail);
        listRequest.LabelIds = "INBOX";
        listRequest.IncludeSpamTrash = true;
        listRequest.Q = $"{attribute}:{value}";

        IMessage? message = await GetLastMessage(listRequest);

        Logger.Information("Finished getting the message... {@message}", JsonSerializer.Serialize(message));
        return message;
    }

    public async Task<IEnumerable<IMessage>> GetMessages()
    {
        Logger.Information("Getting messages...");
        Logger.Information("ownerGmail value is: {ownerGmail}", EmailProviderOptions.OwnerGmail);

        UsersResource.MessagesResource.ListRequest listRequest = Service.Users.Messages.List(EmailProviderOptions.OwnerGmail);
        listRequest.LabelIds = "INBOX";
        listRequest.IncludeSpamTrash = true;

        IEnumerable<IMessage> messages = await GetMessages(listRequest);

        Logger.Information("Finished getting messages... {@messages}", messages);
        return messages;
    }

    public async Task<IEnumerable<IMessage>> GetMessages(string from)
    {
        Logger.Information("Getting messages...");
        Logger.Information("ownerGmail value is: {ownerGmail}", EmailProviderOptions.OwnerGmail);

        UsersResource.MessagesResource.ListRequest listRequest = Service.Users.Messages.List(EmailProviderOptions.OwnerGmail);
        listRequest.LabelIds = "INBOX";
        listRequest.IncludeSpamTrash = true;
        listRequest.Q = $"from:{from}";

        IEnumerable<IMessage> messages = await GetMessages(listRequest);

        Logger.Information("Finished getting messages... {@messages}", messages);
        return messages;
    }

    private async Task<IEnumerable<IMessage>> GetMessages(UsersResource.MessagesResource.ListRequest listRequest)
    {
        IEnumerable<IMessage> messages = Array.Empty<IMessage>();

        ListMessagesResponse listResponse = await listRequest.ExecuteAsync();

        if (listResponse == null || listResponse.Messages == null || !listResponse.Messages.Any())
            return messages;

        foreach (GmailMessage gmailMessage in listResponse.Messages)
        {
            IMessage? message = ProcessMessage(gmailMessage);
            if (message is null)
                continue;

            messages = messages.Append(message);
        }

        return messages;
    }

    private IMessage? ProcessMessage(GmailMessage message)
    {
        string fromAddress = string.Empty;
        string date = string.Empty;
        string subject = string.Empty;
        string mailBody;
        string base64DecodedMailBody;

        foreach (MessagePartHeader header in message.Payload.Headers)
            switch (header.Name)
            {
                case "Form":
                    fromAddress = header.Value;
                    break;
                case "Date":
                    date = header.Value;
                    break;
                case "Subject":
                    subject = header.Value;
                    break;
            }

        if (message.Payload.Parts == null && message.Payload.Body != null)
            mailBody = message.Payload.Body.Data;
        else
            mailBody = GmailApiHelper.MsgNestedParts(message.Payload.Parts ?? throw new NullReferenceException("Failed to set mail's body."));

        base64DecodedMailBody = GmailApiHelper.Base64Decode(mailBody);

        if (!string.IsNullOrEmpty(base64DecodedMailBody))
            return new Message()
            {
                Id = message.Id,
                From = fromAddress,
                To = EmailProviderOptions.OwnerGmail,
                Subject = subject,
                Body = base64DecodedMailBody,
                ETag = message.ETag,
                SentAt = DateTime.Parse(date.Contains('(') ? date[..(date.IndexOf('(') - 1)] : date)
            };

        return null;
    }

    public async Task<bool> DeleteMessage(string id)
    {
        Logger.Information("Deleting the message...");
        Logger.Information("The message id: {id}", id);

        string response = await Service.Users.Messages.Delete(EmailProviderOptions.OwnerGmail, id).ExecuteAsync();

        if (!string.IsNullOrEmpty(response))
        {
            Logger.Error("Failure while trying to delete message with id: {id}", id);
            return false;
        }

        Logger.Information("Finished deleting the message...");
        return true;
    }

    public async Task<bool> DeleteMessages(string from)
    {
        IEnumerable<IMessage> messages = await GetMessages(from);

        Logger.Information("Deleting all of the messages...");

        IEnumerable<string> ids = messages.ToList().ConvertAll(x => x.Id);

        if (!ids.Any())
        {
            Logger.Information("There is no message to delete...");
            return false;
        }

        string response = await Service.Users.Messages.BatchDelete(new BatchDeleteMessagesRequest() { Ids = ids.ToList() }, EmailProviderOptions.OwnerGmail).ExecuteAsync();

        if (!string.IsNullOrEmpty(response))
        {
            Logger.Error("Failure while trying to delete messages. from: {from}", from);
            return false;
        }

        Logger.Information("Finished deleting all of the messages...");
        return true;
    }

    public async Task<bool> DeleteMessages(IEnumerable<string> ids)
    {
        Logger.Information("Deleting all of the messages...");

        if (!ids.Any())
        {
            Logger.Information("There is no message to delete...");
            return false;
        }

        string response = await Service.Users.Messages.BatchDelete(new BatchDeleteMessagesRequest() { Ids = ids.ToList() }, EmailProviderOptions.OwnerGmail).ExecuteAsync();

        if (!string.IsNullOrEmpty(response))
        {
            Logger.Error("Failure while trying to delete emails.");
            return false;
        }

        Logger.Information("Finished deleting all of the emails...");
        return true;
    }
}
