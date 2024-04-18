using Microsoft.AspNetCore.Authentication;

namespace bot.src.Authentication.ApiKey;

public class ApiKeyAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    public string ApiKey { get; set; } = null!;
}
