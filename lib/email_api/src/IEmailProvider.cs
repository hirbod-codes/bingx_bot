using email_api.Models;

namespace email_api.src;

public interface IEmailProvider
{
    public string GetSignalProviderEmail();
    public string GetProviderEmail();
    public Task<Email?> GetEmail(string id);
    public Task<Email?> GetLastEmail(string? filterByEmail = null);
    public Task<IEnumerable<Email>> GetEmails(string? filterByEmail = null);
    public Task DeleteEmail(string id);
    public Task DeleteEmails(string? filterByEmail = null);
}
