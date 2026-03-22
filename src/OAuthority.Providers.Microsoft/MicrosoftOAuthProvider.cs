using OAuthority;

namespace OAuthority.Providers;

public record MicrosoftOAuthOptions
{
    public required string ClientId { get; init; }
    public string TenantId { get; init; } = "common";
    public string? ClientSecret { get; init; }
    public IReadOnlyList<string> Scopes { get; init; } = ["openid", "email", "profile", "offline_access"];
    public int LocalPort { get; init; } = 5000;
}

public class MicrosoftOAuthProvider(MicrosoftOAuthOptions options) : IOAuthProvider
{
    public string Name => "Microsoft";

    public OAuthOptions Options { get; } = new OAuthOptions
    {
        AuthorizationEndpoint = $"https://login.microsoftonline.com/{options.TenantId}/oauth2/v2.0/authorize",
        TokenEndpoint = $"https://login.microsoftonline.com/{options.TenantId}/oauth2/v2.0/token",
        ClientId = options.ClientId,
        ClientSecret = options.ClientSecret,
        RedirectUri = $"http://localhost:{options.LocalPort}/callback",
        Scopes = options.Scopes,
        UsePkce = true,
    };
}