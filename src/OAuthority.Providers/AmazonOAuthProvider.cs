using OAuthority;

namespace OAuthority.Providers;

/// <remarks>
/// Uses Login with Amazon (LwA). PKCE is not supported and will always be disabled
/// regardless of <see cref="ProviderConfig.UsePkce"/>.
/// </remarks>
internal sealed class AmazonOAuthProvider(ProviderConfig config) : IOAuthProvider
{
    private static readonly IReadOnlyList<string> DefaultScopes = ["profile"];

    public string Name => "Amazon";

    public OAuthOptions Options { get; } = new OAuthOptions
    {
        AuthorizationEndpoint = "https://www.amazon.com/ap/oa",
        TokenEndpoint = "https://api.amazon.com/auth/o2/token",
        ClientId = config.ClientId,
        ClientSecret = config.ClientSecret,
        RedirectUri = config.RedirectUri ?? "http://localhost:5000/callback",
        Scopes = config.Scopes ?? DefaultScopes,
        UsePkce = false, // Login with Amazon does not support PKCE
        ExtraParameters = config.ExtraParameters ?? new Dictionary<string, string>(),
    };
}
