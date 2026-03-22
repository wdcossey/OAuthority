using OAuthority;

namespace OAuthority.Providers;

internal sealed class LinkedInOAuthProvider(ProviderConfig config) : IOAuthProvider
{
    private static readonly IReadOnlyList<string> DefaultScopes = ["openid", "profile", "email"];

    public string Name => "LinkedIn";

    public OAuthOptions Options { get; } = new OAuthOptions
    {
        AuthorizationEndpoint = "https://www.linkedin.com/oauth/v2/authorization",
        TokenEndpoint = "https://www.linkedin.com/oauth/v2/accessToken",
        ClientId = config.ClientId,
        ClientSecret = config.ClientSecret,
        RedirectUri = config.RedirectUri ?? "http://localhost:5000/callback",
        Scopes = config.Scopes ?? DefaultScopes,
        UsePkce = false, // LinkedIn does not support PKCE
        ExtraParameters = config.ExtraParameters ?? new Dictionary<string, string>(),
    };
}
