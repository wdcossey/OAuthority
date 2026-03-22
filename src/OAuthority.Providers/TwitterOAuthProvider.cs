using OAuthority;

namespace OAuthority.Providers;

/// <remarks>
/// Uses the Twitter/X OAuth 2.0 API (v2). PKCE is required and always enforced regardless of
/// <see cref="ProviderConfig.UsePkce"/>.
/// </remarks>
internal sealed class TwitterOAuthProvider(ProviderConfig config) : IOAuthProvider
{
    private static readonly IReadOnlyList<string> DefaultScopes = ["tweet.read", "users.read", "offline.access"];

    public string Name => "Twitter";

    public OAuthOptions Options { get; } = new OAuthOptions
    {
        AuthorizationEndpoint = "https://twitter.com/i/oauth2/authorize",
        TokenEndpoint = "https://api.twitter.com/2/oauth2/token",
        ClientId = config.ClientId,
        ClientSecret = config.ClientSecret,
        RedirectUri = config.RedirectUri ?? "http://localhost:5000/callback",
        Scopes = config.Scopes ?? DefaultScopes,
        UsePkce = true, // Twitter OAuth 2.0 requires PKCE
        ExtraParameters = config.ExtraParameters ?? new Dictionary<string, string>(),
    };
}
