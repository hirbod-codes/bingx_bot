using Microsoft.AspNetCore.Authentication;

namespace CandleRepository.src.Authentication.ApiKey;

public class ApiKeyAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    public string ApiKey { get; set; } = null!;
}
