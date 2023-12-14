using System.Text.Json;
using email_api.Exceptions;
using email_api.Models;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Serilog.Core;

namespace email_api.src.Providers;

public class GmailProvider : IEmailProvider
{
    public GmailService Service { get; private set; }
    public EmailProviderOptions EmailProviderOptions { get; }
    public string AccessToken { get; private set; } = null!;
    public string RefreshToken { get; private set; } = null!;

    private Logger Logger { get; }


    public GmailProvider(EmailProviderOptions emailProviderOptions, Logger logger)
    {
        EmailProviderOptions = emailProviderOptions;
        (GmailService service, string accessToken, string refreshToken) = GmailApiHelper.Authenticate(emailProviderOptions);
        Service = service;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        Logger = logger;
    }

    public string GetSignalProviderEmail() => EmailProviderOptions.SignalProviderEmail;

    public string GetProviderEmail() => EmailProviderOptions.OwnerGmail;

    public async Task<Email?> GetEmail(string id)
    {
        Logger.Information("Getting the email...");
        Logger.Information("ownerGmail value is: {ownerGmail}", EmailProviderOptions.OwnerGmail);

        UsersResource.MessagesResource.GetRequest getRequest = Service.Users.Messages.Get(EmailProviderOptions.OwnerGmail, id);
        Message message = await getRequest.ExecuteAsync();
        Email? email = await ProcessMessage(message);

        Logger.Information("Found the email: {@email}", JsonSerializer.Serialize(email));
        Logger.Information("Getting the email...");
        return email;
    }

    public Task<Email?> ProcessMessage(Message message)
    {
        UsersResource.MessagesResource.GetRequest messageRequest = Service.Users.Messages.Get(EmailProviderOptions.OwnerGmail, message.Id);

        //MAKE ANOTHER REQUEST FOR THAT EMAIL ID...
        Message executedMessage = messageRequest.Execute();

        string fromAddress = string.Empty;
        string date = string.Empty;
        string subject;
        string mailBody;
        string readableText;

        //LOOP THROUGH THE HEADERS AND GET THE FIELDS WE NEED (SUBJECT, MAIL)
        foreach (MessagePartHeader header in executedMessage.Payload.Headers)
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

        //READ MAIL BODY-------------------------------------------------------------------------------------
        if (executedMessage.Payload.Parts == null && executedMessage.Payload.Body != null)
            mailBody = executedMessage.Payload.Body.Data;
        else
            mailBody = GmailApiHelper.MsgNestedParts(executedMessage.Payload.Parts ?? throw new NullReferenceException("Failed to set mail's body."));

        readableText = GmailApiHelper.Base64Decode(mailBody);

        if (!string.IsNullOrEmpty(readableText))
        {
            Email email = new()
            {
                From = fromAddress,
                Body = readableText,
                Id = message.Id,
                To = EmailProviderOptions.OwnerGmail,
                ETag = message.ETag,
                MailDateTime = DateTime.Parse(date.Contains('(') ? date[..(date.IndexOf('(') - 1)] : date)
            };
            return Task.FromResult<Email?>(email);
        }

        return Task.FromResult<Email?>(null);
    }

    public async Task<Email?> GetLastEmail(string? filterByEmail = null)
    {
        Logger.Information("Getting the last email...");
        Logger.Information("ownerGmail value is: {ownerGmail}", EmailProviderOptions.OwnerGmail);
        Logger.Information("filterByEmail value is: {filterByEmail}", filterByEmail);

        UsersResource.MessagesResource.ListRequest listRequest = Service.Users.Messages.List(EmailProviderOptions.OwnerGmail);
        listRequest.LabelIds = "INBOX";
        listRequest.IncludeSpamTrash = false;
        if (filterByEmail != null)
            listRequest.Q = $"from:{filterByEmail}";

        //GET ALL EMAILS
        ListMessagesResponse listResponse = await listRequest.ExecuteAsync();

        if (listResponse == null || listResponse.Messages == null)
        {
            Logger.Information("Finished getting the last email, No email found...");
            return null;
        }

        //LOOP THROUGH EACH EMAIL AND GET WHAT FIELDS I WANT
        foreach (Message message in listResponse.Messages)
        {
            Email? email = await ProcessMessage(message);
            if (email is not null)
            {
                Logger.Information("Finished getting the last email... {@EmailList}", JsonSerializer.Serialize(email));
                return email;
            }
        }

        Logger.Information("Finished getting the last email, No email found...");
        return null;
    }

    public async Task<IEnumerable<Email>> GetEmails(string? filterByEmail = null)
    {
        Logger.Information("Getting all the emails...");
        Logger.Information("ownerGmail value is: {ownerGmail}", EmailProviderOptions.OwnerGmail);
        Logger.Information("filterByEmail value is: {filterByEmail}", filterByEmail);

        IEnumerable<Email> emails = Array.Empty<Email>();
        UsersResource.MessagesResource.ListRequest listRequest = Service.Users.Messages.List(EmailProviderOptions.OwnerGmail);
        listRequest.LabelIds = "INBOX";
        listRequest.IncludeSpamTrash = false;
        if (filterByEmail != null)
            listRequest.Q = $"from:{filterByEmail}";

        //GET ALL EMAILS
        ListMessagesResponse listResponse = await listRequest.ExecuteAsync();

        if (listResponse == null || listResponse.Messages == null)
        {
            Logger.Information("Finished getting all the emails...");
            return emails;
        }

        //LOOP THROUGH EACH EMAIL AND GET WHAT FIELDS I WANT
        foreach (Message message in listResponse.Messages)
        {
            Email? email = await ProcessMessage(message);
            if (email is not null)
                emails = emails.Append(email);
        }

        Logger.Information("Finished getting all the emails... {@EmailList}", JsonSerializer.Serialize(emails));
        return emails;
    }

    public async Task DeleteEmail(string id)
    {
        Logger.Information("Deleting the email...");
        Logger.Information("The email id: {id}", id);

        Email? email = await GetEmail(id);

        if (email is null)
        {
            Logger.Information("Finished deleting the email(email not found)...");
            return;
        }

        string response = await Service.Users.Messages.Delete(EmailProviderOptions.OwnerGmail, id).ExecuteAsync();

        if (!string.IsNullOrEmpty(response))
        {
            Logger.Error("Failure while trying to delete email with email id: {id}", id);
            throw new EmailsDeletionException($"Failure while trying to delete email with email id: {id}");
        }

        Logger.Information("Finished deleting the email...");
    }

    public async Task DeleteEmails(string? filterByEmail = null)
    {
        IEnumerable<Email> emails = await GetEmails(filterByEmail);

        Logger.Information("Deleting all of the emails...");

        IEnumerable<string> Ids = emails.ToList().ConvertAll(x => x.Id);

        if (!Ids.Any())
        {
            Logger.Information("There is no email to delete...");
            return;
        }

        string response = await Service.Users.Messages.BatchDelete(new BatchDeleteMessagesRequest() { Ids = Ids.ToList() }, EmailProviderOptions.OwnerGmail).ExecuteAsync();

        if (!string.IsNullOrEmpty(response))
        {
            Logger.Error("Failure while trying to delete emails. filterByEmail: {filterByEmail}", filterByEmail);
            throw new EmailsDeletionException($"Failure while trying to delete emails. filterByEmail: {filterByEmail}");
        }

        Logger.Information("Finished deleting all of the emails...");
    }
}
