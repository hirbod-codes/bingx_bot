namespace email_api.Models;

using System;
using System.Collections.Generic;

public class Email
{
    public string From { get; set; } = null!;
    public string To { get; set; } = null!;
    public string Body { get; set; } = null!;
    public DateTime MailDateTime { get; set; }
    public List<string> Attachments { get; set; } = new();
    public string Id { get; set; } = null!;
    public string ETag { get; set; } = null!;
}
