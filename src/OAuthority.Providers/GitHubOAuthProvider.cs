using OAuthority;

namespace OAuthority.Providers;

internal sealed class GitHubOAuthProvider(ProviderConfig config) : IOAuthProvider
{
    private static readonly IReadOnlyList<string> DefaultScopes = ["read:user", "user:email"];

    public string Name => "GitHub";

    public OAuthOptions Options { get; } = new OAuthOptions
    {
        AuthorizationEndpoint = "https://github.com/login/oauth/authorize",
        TokenEndpoint = "https://github.com/login/oauth/access_token",
        ClientId = config.ClientId,
        ClientSecret = config.ClientSecret,
        RedirectUri = config.RedirectUri ?? "http://localhost:5000/callback",
        Scopes = config.Scopes ?? DefaultScopes,
        UsePkce = false, // GitHub does not support PKCE
        ExtraParameters = config.ExtraParameters ?? new Dictionary<string, string>(),
    };
}
